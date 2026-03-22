using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Chinook.API.Features.Health.Checks;

public sealed class RedisRoundTripHealthCheck(IDistributedCache cache, IConfiguration configuration)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return HealthCheckResult.Degraded(
                "Connection string 'Redis' is missing. Redis check is degraded.");
        }

        var probeKey = $"health:redis:{Guid.NewGuid():N}";
        var probeValue = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        try
        {
            await cache.SetStringAsync(
                probeKey,
                probeValue,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15)
                },
                cancellationToken);

            var cachedValue = await cache.GetStringAsync(probeKey, cancellationToken);
            var isRoundTripSuccessful = string.Equals(probeValue, cachedValue, StringComparison.Ordinal);

            if (!isRoundTripSuccessful)
            {
                return HealthCheckResult.Unhealthy(
                    "Redis round-trip failed because the stored value did not match.");
            }

            await cache.RemoveAsync(probeKey, cancellationToken);

            return HealthCheckResult.Healthy(
                "Redis round-trip succeeded.",
                new Dictionary<string, object> { ["key"] = probeKey });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Redis round-trip check failed.",
                ex);
        }
    }
}
