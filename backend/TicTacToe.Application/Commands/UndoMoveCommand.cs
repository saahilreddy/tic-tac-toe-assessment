using MediatR;
using TicTacToe.Application.Contracts;
using TicTacToe.Application.Interfaces;
using TicTacToe.Application.Exceptions;
using TicTacToe.Domain;

namespace TicTacToe.Application.Commands;

public sealed record UndoMoveCommand(Guid GameId, long ExpectedVersion) : IRequest<OperationResult<GameStateModel>>;

public sealed class UndoMoveCommandHandler(
    IGameRepository gameRepository,
    IScoreboardRepository scoreboardRepository,
    IApplicationMapper mapper)
    : IRequestHandler<UndoMoveCommand, OperationResult<GameStateModel>>
{
    public async Task<OperationResult<GameStateModel>> Handle(UndoMoveCommand request, CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetAsync(request.GameId, cancellationToken);
        if (game is null)
            return OperationResult<GameStateModel>.Failure(ErrorType.NotFound, $"Game '{request.GameId}' was not found.");

        if (game.IsCompleted)
            return OperationResult<GameStateModel>.Failure(ErrorType.Validation, "Undo is not available after a game is completed.");

        if (game.Moves.Count == 0)
            return OperationResult<GameStateModel>.Failure(ErrorType.Validation, "There are no moves to undo.");

        var movesToRemove = game.Mode == GameMode.Computer ? 2 : 1;
        if (game.Moves.Count < movesToRemove)
            return OperationResult<GameStateModel>.Failure(ErrorType.Validation, "There are not enough moves to undo for the selected game mode.");

        game.UndoLastMoves(movesToRemove);
        if (!await gameRepository.TryUpdateAsync(game, request.ExpectedVersion, cancellationToken))
            throw new ConcurrencyConflictException("The game changed before undo could be saved. Refresh the game and try again.");

        var scoreboard = await scoreboardRepository.GetAsync(cancellationToken);
        return OperationResult<GameStateModel>.Success(mapper.ToGameState(game, scoreboard));
    }
}
