using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.DTOs;
using TicTacToe.Application.Commands;
using TicTacToe.Application.Queries;

namespace TicTacToe.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/scoreboard")]
[Produces("application/json")]
public sealed class ScoreboardController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ScoreboardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ScoreboardResponse>> GetScoreboard(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetScoreboardQuery(), cancellationToken);
        return Ok(result.ToResponse());
    }

    [HttpPost("reset")]
    [ProducesResponseType(typeof(ScoreboardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ScoreboardResponse>> ResetScoreboard(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ResetScoreboardCommand(), cancellationToken);
        return Ok(result.ToResponse());
    }
}
