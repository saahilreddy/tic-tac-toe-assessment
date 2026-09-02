using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TicTacToe.Application.Behaviors;
using TicTacToe.Application.Interfaces;
using TicTacToe.Application.Mappings;

namespace TicTacToe.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddSingleton<IApplicationMapper, ApplicationMapper>();

        return services;
    }
}
