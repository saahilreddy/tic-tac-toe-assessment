using Microsoft.Extensions.DependencyInjection;
using TicTacToe.Application.Interfaces;
using TicTacToe.Domain;
using TicTacToe.Infrastructure.Health;
using TicTacToe.Infrastructure.Persistence;

namespace TicTacToe.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddSingleton<IGameRepository, InMemoryGameRepository>();
        services.AddSingleton<IScoreboardRepository, InMemoryScoreboardRepository>();
        services.AddSingleton<ComputerMoveSelector>();

        services.AddHealthChecks()
            .AddCheck<InMemoryGameStoreHealthCheck>("game-store");

        return services;
    }
}
