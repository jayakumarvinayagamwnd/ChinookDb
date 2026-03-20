using AutoMapper;

namespace Chinook.API.Common.DependencyInjection;

public static class AutoMapperServiceCollections
{
    public static IServiceCollection AddAutoMapperServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(AutoMapperServiceCollections).Assembly);
        return services;
    }
}
