using MediatR;
using TicTacToe.Application.Contracts;
using TicTacToe.Application.Interfaces;

namespace TicTacToe.Application.Queries;

public sealed record GetGameQuery(Guid GameId) : IRequest<OperationResult<GameStateModel>>;

public sealed class GetGameQueryHandler(
    IGameRepository gameRepository,
    IScoreboardRepository scoreboardRepository,
    IApplicationMapper mapper)
    : IRequestHandler<GetGameQuery, OperationResult<GameStateModel>>
{
    public async Task<OperationResult<GameStateModel>> Handle(GetGameQuery request, CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetAsync(request.GameId, cancellationToken);
        if (game is null)
            return OperationResult<GameStateModel>.Failure(ErrorType.NotFound, $"Game '{request.GameId}' was not found.");

        return OperationResult<GameStateModel>.Success(
            mapper.ToGameState(game, await scoreboardRepository.GetAsync(cancellationToken)));
    }
}
