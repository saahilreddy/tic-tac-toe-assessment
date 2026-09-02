using TicTacToe.Application.Contracts;
using TicTacToe.Application.Interfaces;
using TicTacToe.Domain;

namespace TicTacToe.Application.Mappings;

public sealed class ApplicationMapper : IApplicationMapper
{
    public GameStateModel ToGameState(Game game, ScoreboardSnapshot scoreboard) =>
        new(
            game.Id,
            game.Version,
            game.Board.ToArray(),
            game.CurrentPlayer,
            game.Mode,
            game.Status,
            game.Winner,
            game.WinningCells.ToArray(),
            game.Moves.Select(move => new MoveModel(
                move.MoveNumber,
                move.Player,
                move.Row,
                move.Column,
                move.CellIndex)).ToArray(),
            ToScoreboard(scoreboard));

    public ScoreboardModel ToScoreboard(ScoreboardSnapshot scoreboard) =>
        new(scoreboard.XWins, scoreboard.OWins, scoreboard.Draws);
}
