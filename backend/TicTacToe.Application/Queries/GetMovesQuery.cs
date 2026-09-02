using MediatR;
using TicTacToe.Application.Contracts;
using TicTacToe.Application.Interfaces;

namespace TicTacToe.Application.Queries;

public sealed record GetMovesQuery(Guid GameId) : IRequest<OperationResult<IReadOnlyList<MoveModel>>>;

public sealed class GetMovesQueryHandler(IGameRepository gameRepository)
    : IRequestHandler<GetMovesQuery, OperationResult<IReadOnlyList<MoveModel>>>
{
    public async Task<OperationResult<IReadOnlyList<MoveModel>>> Handle(GetMovesQuery request, CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetAsync(request.GameId, cancellationToken);
        if (game is null)
            return OperationResult<IReadOnlyList<MoveModel>>.Failure(ErrorType.NotFound, $"Game '{request.GameId}' was not found.");

        IReadOnlyList<MoveModel> moves = game.Moves
            .Select(m => new MoveModel(m.MoveNumber, m.Player, m.Row, m.Column, m.CellIndex))
            .ToArray();
        return OperationResult<IReadOnlyList<MoveModel>>.Success(moves);
    }
}
