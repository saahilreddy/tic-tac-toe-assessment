using System.ComponentModel.DataAnnotations;
using TicTacToe.Domain;

namespace TicTacToe.Api.DTOs;

public sealed record CreateGameRequest(GameMode Mode);

public sealed record MoveRequest(
    Guid GameId,
    Player Player,
    [Range(0, 2)] int Row,
    [Range(0, 2)] int Column,
    [Range(0, long.MaxValue)] long ExpectedVersion);

public sealed record VersionRequest(
    [Range(0, long.MaxValue)] long ExpectedVersion);
