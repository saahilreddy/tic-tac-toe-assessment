namespace TicTacToe.Application.Interfaces;

public interface IScoreboardRepository
{
    Task<ScoreboardSnapshot> GetAsync(CancellationToken cancellationToken = default);
    Task ApplyGameResultOnceAsync(Guid gameId, TicTacToe.Domain.GameOutcome outcome, CancellationToken cancellationToken = default);
    Task ResetAsync(CancellationToken cancellationToken = default);
}

public sealed record ScoreboardSnapshot(int XWins, int OWins, int Draws);
