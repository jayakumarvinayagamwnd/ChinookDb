namespace Chinook.API.Common.Contracts.Queries;

/// <summary>
/// Marks a query as cacheable. The MediatR CachingBehavior reads these
/// properties to build the cache key and set the expiry on IDistributedCache.
/// </summary>
public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan? Expiry { get; }
}
