using TicTacToe.Domain;

namespace TicTacToe.Api.DTOs;

public sealed record MoveResponse(
    int MoveNumber,
    Player Player,
    int Row,
    int Column,
    int CellIndex);

public sealed record ScoreboardResponse(
    int XWins,
    int OWins,
    int Draws);

public sealed record GameStateResponse(
    Guid GameId,
    long Version,
    IReadOnlyList<string> Board,
    Player CurrentPlayer,
    GameMode GameMode,
    GameStatus GameStatus,
    Player? Winner,
    IReadOnlyList<int> WinningCells,
    IReadOnlyList<MoveResponse> MoveHistory,
    ScoreboardResponse Scoreboard);
