using System.Reflection;
using System.Text.Json;
using Chinook.API.Common.Contracts.Queries;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Chinook.API.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that transparently caches responses for any
/// request that implements <see cref="ICacheableQuery"/> using IDistributedCache.
/// Runs after ValidationBehavior (registered after it in DI), so invalid
/// requests are rejected before any cache interaction.
/// </summary>
public sealed class CachingBehavior<TRequest, TResponse>(
    IDistributedCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheable)
            return await next();

        var cacheKey = cacheable.CacheKey;

        var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            logger.LogInformation("[CachingBehavior] Cache hit for key: {CacheKey}", cacheKey);

            if (IsFluentResultResponseType())
            {
                if (TryBuildSuccessfulResultFromCachedPayload(cached, out var cachedResult))
                    return cachedResult!;

                // If the cached payload cannot be read, clear it and continue to handler.
                await cache.RemoveAsync(cacheKey, cancellationToken);
                logger.LogWarning("[CachingBehavior] Removed invalid cached payload for key: {CacheKey}", cacheKey);
            }
            else
            {
                var deserialized = JsonSerializer.Deserialize<TResponse>(cached);
                if (deserialized is not null)
                    return deserialized;

                await cache.RemoveAsync(cacheKey, cancellationToken);
                logger.LogWarning("[CachingBehavior] Removed null-deserialized cache entry for key: {CacheKey}", cacheKey);
            }
        }

        logger.LogInformation("[CachingBehavior] Cache miss for key: {CacheKey}", cacheKey);

        var response = await next();

        if (response is not null)
        {
            var options = new DistributedCacheEntryOptions();
            if (cacheable.Expiry.HasValue)
                options.AbsoluteExpirationRelativeToNow = cacheable.Expiry;

            if (IsFluentResultResponseType())
            {
                if (TryExtractSuccessfulResultPayload(response, out var payloadToCache))
                {
                    await cache.SetStringAsync(
                        cacheKey,
                        payloadToCache!,
                        options,
                        cancellationToken);
                }
                else
                {
                    logger.LogInformation("[CachingBehavior] Skipped caching failed result for key: {CacheKey}", cacheKey);
                }
            }
            else
            {
                await cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(response),
                    options,
                    cancellationToken);
            }
        }

        return response;
    }

    private static bool IsFluentResultResponseType()
    {
        var responseType = typeof(TResponse);
        return responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>);
    }

    private static bool TryExtractSuccessfulResultPayload(TResponse response, out string? payloadToCache)
    {
        payloadToCache = null;

        if (response is not IResultBase resultBase || resultBase.IsFailed)
            return false;

        var responseType = typeof(TResponse);
        var valueType = responseType.GetGenericArguments()[0];
        var valueProperty = responseType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
        var value = valueProperty?.GetValue(response);

        payloadToCache = JsonSerializer.Serialize(value, valueType);
        return true;
    }

    private static bool TryBuildSuccessfulResultFromCachedPayload(string cachedPayload, out TResponse? result)
    {
        result = default;
        try
        {
            var responseType = typeof(TResponse);
            var valueType = responseType.GetGenericArguments()[0];
            var value = JsonSerializer.Deserialize(cachedPayload, valueType);

            var okMethod = typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(Result.Ok) && m.IsGenericMethodDefinition && m.GetParameters().Length == 1)
                .MakeGenericMethod(valueType);

            var response = okMethod.Invoke(null, [value]);
            if (response is null)
                return false;

            result = (TResponse)response;
            return true;
        }
        catch
        {
            // Stale payload format from earlier builds; caller will evict and refresh.
            return false;
        }
    }
}
