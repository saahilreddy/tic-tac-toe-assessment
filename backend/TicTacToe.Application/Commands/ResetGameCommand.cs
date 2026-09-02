using MediatR;
using TicTacToe.Application.Contracts;
using TicTacToe.Application.Interfaces;
using TicTacToe.Application.Exceptions;
using TicTacToe.Domain;

namespace TicTacToe.Application.Commands;

public sealed record ResetGameCommand(Guid GameId, long ExpectedVersion) : IRequest<OperationResult<GameStateModel>>;

public sealed class ResetGameCommandHandler(
    IGameRepository gameRepository,
    IScoreboardRepository scoreboardRepository,
    IApplicationMapper mapper)
    : IRequestHandler<ResetGameCommand, OperationResult<GameStateModel>>
{
    public async Task<OperationResult<GameStateModel>> Handle(ResetGameCommand request, CancellationToken cancellationToken)
    {
        var current = await gameRepository.GetAsync(request.GameId, cancellationToken);
        if (current is null)
            return OperationResult<GameStateModel>.Failure(ErrorType.NotFound, $"Game '{request.GameId}' was not found.");

        if (!await gameRepository.TryDeleteAsync(request.GameId, request.ExpectedVersion, cancellationToken))
            throw new ConcurrencyConflictException("The game changed before reset could be saved. Refresh the game and try again.");

        var newGame = new Game(Guid.NewGuid(), current.Mode);
        await gameRepository.AddAsync(newGame, cancellationToken);
        var scoreboard = await scoreboardRepository.GetAsync(cancellationToken);
        return OperationResult<GameStateModel>.Success(mapper.ToGameState(newGame, scoreboard));
    }
}
