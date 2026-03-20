using MediatR;

namespace Chinook.API.Common.DependencyInjection;
public static class MediatRServiceCollections
{
    public static IServiceCollection AddMediatRServices(this IServiceCollection services)
    {
        services.AddMediatR(typeof(MediatRServiceCollections).Assembly);
        return services;
    }
}