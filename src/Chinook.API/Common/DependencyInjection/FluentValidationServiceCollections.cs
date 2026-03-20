using Chinook.API.Common.Behaviors;
using FluentValidation;
using MediatR;

namespace Chinook.API.Common.DependencyInjection;

public static class FluentValidationServiceCollections
{
    public static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(FluentValidationServiceCollections).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}
