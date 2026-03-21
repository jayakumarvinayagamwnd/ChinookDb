using Chinook.API.Common.Behaviors;
using MediatR;

namespace Chinook.API.Infrastructure.Caching;

public static class CacheServiceCollectionExtensions
{
    public static IServiceCollection AddCacheServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "Chinook:";
        });

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        return services;
    }
}
