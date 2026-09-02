using System.Collections.Concurrent;
using TicTacToe.Application.Interfaces;
using TicTacToe.Domain;

namespace TicTacToe.Infrastructure.Persistence;

public sealed class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<Guid, Game> _games = new();

    public int Count => _games.Count;

    public Task<Game?> GetAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_games.TryGetValue(gameId, out var game) ? game.Clone() : null);
    }

    public Task AddAsync(Game game, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_games.TryAdd(game.Id, game.Clone()))
            throw new InvalidOperationException($"Game '{game.Id}' already exists.");
        return Task.CompletedTask;
    }

    public Task<bool> TryUpdateAsync(Game game, long expectedVersion, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_games.TryGetValue(game.Id, out var existing))
            return Task.FromResult(false);

        if (existing.Version != expectedVersion)
            return Task.FromResult(false);

        return Task.FromResult(_games.TryUpdate(game.Id, game.Clone(), existing));
    }

    public Task<bool> TryDeleteAsync(Guid gameId, long expectedVersion, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        while (_games.TryGetValue(gameId, out var existing))
        {
            if (existing.Version != expectedVersion)
                return Task.FromResult(false);

            if (((ICollection<KeyValuePair<Guid, Game>>)_games).Remove(new KeyValuePair<Guid, Game>(gameId, existing)))
                return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
