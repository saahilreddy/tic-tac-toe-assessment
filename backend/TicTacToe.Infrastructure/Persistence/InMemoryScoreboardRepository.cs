using TicTacToe.Application.Interfaces;
using TicTacToe.Domain;

namespace TicTacToe.Infrastructure.Persistence;

public sealed class InMemoryScoreboardRepository : IScoreboardRepository
{
    private readonly object _sync = new();
    private readonly HashSet<Guid> _appliedGameResults = [];
    private int _xWins;
    private int _oWins;
    private int _draws;

    public Task<ScoreboardSnapshot> GetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
            return Task.FromResult(new ScoreboardSnapshot(_xWins, _oWins, _draws));
    }

    public Task ApplyGameResultOnceAsync(Guid gameId, GameOutcome outcome, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (outcome == GameOutcome.InProgress)
            return Task.CompletedTask;

        lock (_sync)
        {
            if (!_appliedGameResults.Add(gameId))
                return Task.CompletedTask;

            switch (outcome)
            {
                case GameOutcome.XWon:
                    _xWins++;
                    break;
                case GameOutcome.OWon:
                    _oWins++;
                    break;
                case GameOutcome.Draw:
                    _draws++;
                    break;
            }
        }

        return Task.CompletedTask;
    }

    public Task ResetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            _xWins = 0;
            _oWins = 0;
            _draws = 0;
            _appliedGameResults.Clear();
        }

        return Task.CompletedTask;
    }
}
