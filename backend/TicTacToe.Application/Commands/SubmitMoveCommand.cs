using MediatR;
using TicTacToe.Application.Contracts;
using TicTacToe.Application.Interfaces;
using TicTacToe.Application.Exceptions;
using TicTacToe.Domain;

namespace TicTacToe.Application.Commands;

public sealed record SubmitMoveCommand(
    Guid GameId,
    Guid RequestGameId,
    Player Player,
    int Row,
    int Column,
    long ExpectedVersion) : IRequest<OperationResult<GameStateModel>>;

public sealed class SubmitMoveCommandHandler(
    IGameRepository gameRepository,
    IScoreboardRepository scoreboardRepository,
    IApplicationMapper mapper,
    ComputerMoveSelector computerMoveSelector)
    : IRequestHandler<SubmitMoveCommand, OperationResult<GameStateModel>>
{
    public async Task<OperationResult<GameStateModel>> Handle(SubmitMoveCommand request, CancellationToken cancellationToken)
    {
        if (request.GameId != request.RequestGameId)
            return OperationResult<GameStateModel>.Failure(ErrorType.Validation, "Game ID in the request body does not match the route.");

        var game = await gameRepository.GetAsync(request.GameId, cancellationToken);
        if (game is null)
            return OperationResult<GameStateModel>.Failure(ErrorType.NotFound, $"Game '{request.GameId}' was not found.");

        var move = new Move(game.Moves.Count + 1, request.Player, request.Row, request.Column);
        var validation = GameRules.ValidateMove(game, move);
        if (!validation.IsValid)
            return OperationResult<GameStateModel>.Failure(ErrorType.Validation, validation.ErrorMessage!);

        game.ApplyMove(move);
        var outcome = GameRules.EvaluateOutcome(game);
        if (outcome != GameOutcome.InProgress)
            CompleteGame(game, outcome);

        if (game.Mode == GameMode.Computer && outcome == GameOutcome.InProgress)
        {
            var computerIndex = computerMoveSelector.SelectMove(game.Board);
            if (computerIndex >= 0)
            {
                var computerMove = new Move(
                    game.Moves.Count + 1,
                    Player.O,
                    computerIndex / 3,
                    computerIndex % 3);
                game.ApplyMove(computerMove);
                outcome = GameRules.EvaluateOutcome(game);
                if (outcome != GameOutcome.InProgress)
                    CompleteGame(game, outcome);
            }
        }

        var persisted = await gameRepository.TryUpdateAsync(game, request.ExpectedVersion, cancellationToken);
        if (!persisted)
            throw new ConcurrencyConflictException("The game changed before your move could be saved. Refresh the game and try again.");

        if (outcome != GameOutcome.InProgress)
            await scoreboardRepository.ApplyGameResultOnceAsync(game.Id, outcome, cancellationToken);

        var scoreboard = await scoreboardRepository.GetAsync(cancellationToken);
        return OperationResult<GameStateModel>.Success(mapper.ToGameState(game, scoreboard));
    }

    private static void CompleteGame(Game game, GameOutcome outcome)
    {
        switch (outcome)
        {
            case GameOutcome.XWon:
                var xLine = GameRules.FindWinner(game.Board);
                game.CompleteWin(Player.X, xLine!.Value.Cells);
                break;
            case GameOutcome.OWon:
                var oLine = GameRules.FindWinner(game.Board);
                game.CompleteWin(Player.O, oLine!.Value.Cells);
                break;
            case GameOutcome.Draw:
                game.CompleteDraw();
                break;
        }
    }
}
