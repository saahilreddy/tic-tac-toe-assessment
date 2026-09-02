using TicTacToe.Application.Contracts;

namespace TicTacToe.Api.DTOs;

public static class ApiMapper
{
    public static GameStateResponse ToResponse(this GameStateModel model) =>
        new(
            model.GameId,
            model.Version,
            model.Board,
            model.CurrentPlayer,
            model.GameMode,
            model.GameStatus,
            model.Winner,
            model.WinningCells,
            model.MoveHistory.Select(move => new MoveResponse(
                move.MoveNumber,
                move.Player,
                move.Row,
                move.Column,
                move.CellIndex)).ToArray(),
            model.Scoreboard.ToResponse());

    public static ScoreboardResponse ToResponse(this ScoreboardModel model) =>
        new(model.XWins, model.OWins, model.Draws);

    public static MoveResponse ToResponse(this MoveModel model) =>
        new(model.MoveNumber, model.Player, model.Row, model.Column, model.CellIndex);
}
