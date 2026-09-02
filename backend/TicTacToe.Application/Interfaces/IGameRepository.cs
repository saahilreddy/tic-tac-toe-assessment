using TicTacToe.Domain;

namespace TicTacToe.Application.Interfaces;

public interface IGameRepository
{
    Task<Game?> GetAsync(Guid gameId, CancellationToken cancellationToken = default);
    Task AddAsync(Game game, CancellationToken cancellationToken = default);
    Task<bool> TryUpdateAsync(Game game, long expectedVersion, CancellationToken cancellationToken = default);
    Task<bool> TryDeleteAsync(Guid gameId, long expectedVersion, CancellationToken cancellationToken = default);
    int Count { get; }
}
