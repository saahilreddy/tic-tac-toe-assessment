using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.DTOs;
using TicTacToe.Application.Commands;
using TicTacToe.Application.Contracts;
using TicTacToe.Application.Queries;

namespace TicTacToe.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/games")]
[Produces("application/json")]
public sealed class GamesController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GameStateResponse>> CreateGame(
        [FromBody] CreateGameRequest request,
        CancellationToken cancellationToken)
    {
        var state = await sender.Send(new CreateGameCommand(request.Mode), cancellationToken);
        return CreatedAtAction(nameof(GetGame), new { id = state.GameId, version = "1" }, state.ToResponse());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameStateResponse>> GetGame(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGameQuery(id), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value!.ToResponse())
            : ToError(result.Error!);
    }

    [HttpGet("{id:guid}/state")]
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<GameStateResponse>> GetGameState(Guid id, CancellationToken cancellationToken) =>
        GetGame(id, cancellationToken);

    [HttpGet("{id:guid}/moves")]
    [ProducesResponseType(typeof(IReadOnlyList<MoveResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<MoveResponse>>> GetMoves(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMovesQuery(id), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value!.Select(x => x.ToResponse()).ToArray())
            : ToError(result.Error!);
    }

    [HttpPost("{id:guid}/moves")]
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GameStateResponse>> SubmitMove(
        Guid id,
        [FromBody] MoveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SubmitMoveCommand(id, request.GameId, request.Player, request.Row, request.Column, request.ExpectedVersion),
            cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value!.ToResponse())
            : ToError(result.Error!);
    }

    [HttpPost("{id:guid}/undo")]
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GameStateResponse>> Undo(
        Guid id,
        [FromBody] VersionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UndoMoveCommand(id, request.ExpectedVersion), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value!.ToResponse())
            : ToError(result.Error!);
    }

    [HttpPost("{id:guid}/reset")]
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GameStateResponse>> ResetGame(
        Guid id,
        [FromBody] VersionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ResetGameCommand(id, request.ExpectedVersion), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value!.ToResponse())
            : ToError(result.Error!);
    }

    private ActionResult ToError(ApplicationError error) => error.Type switch
    {
        ErrorType.NotFound => NotFound(new { message = error.Message }),
        ErrorType.Conflict => Conflict(new { message = error.Message }),
        _ => BadRequest(new { message = error.Message })
    };
}
