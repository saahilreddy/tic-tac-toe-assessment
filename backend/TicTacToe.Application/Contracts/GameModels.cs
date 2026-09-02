using TicTacToe.Domain;

namespace TicTacToe.Application.Contracts;

public sealed record MoveModel(
    int MoveNumber,
    Player Player,
    int Row,
    int Column,
    int CellIndex);

public sealed record ScoreboardModel(
    int XWins,
    int OWins,
    int Draws);

public sealed record GameStateModel(
    Guid GameId,
    long Version,
    IReadOnlyList<string> Board,
    Player CurrentPlayer,
    GameMode GameMode,
    GameStatus GameStatus,
    Player? Winner,
    IReadOnlyList<int> WinningCells,
    IReadOnlyList<MoveModel> MoveHistory,
    ScoreboardModel Scoreboard);
