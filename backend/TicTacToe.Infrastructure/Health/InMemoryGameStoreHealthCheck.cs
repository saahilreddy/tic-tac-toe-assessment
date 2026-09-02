using Microsoft.Extensions.Diagnostics.HealthChecks;
using TicTacToe.Application.Interfaces;

namespace TicTacToe.Infrastructure.Health;

public sealed class InMemoryGameStoreHealthCheck(IGameRepository repository) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var gameCount = repository.Count;
            return Task.FromResult(
                HealthCheckResult.Healthy($"In-memory game repository is available. Active games: {gameCount}."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Game repository health check failed.", ex));
        }
    }
}
