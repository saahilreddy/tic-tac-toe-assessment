using MediatR;
using TicTacToe.Application.Contracts;
using TicTacToe.Application.Interfaces;
using TicTacToe.Domain;

namespace TicTacToe.Application.Commands;

public sealed record CreateGameCommand(GameMode Mode) : IRequest<GameStateModel>;

public sealed class CreateGameCommandHandler(
    IGameRepository gameRepository,
    IScoreboardRepository scoreboardRepository,
    IApplicationMapper mapper)
    : IRequestHandler<CreateGameCommand, GameStateModel>
{
    public async Task<GameStateModel> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        var game = new Game(Guid.NewGuid(), request.Mode);
        await gameRepository.AddAsync(game, cancellationToken);
        var scoreboard = await scoreboardRepository.GetAsync(cancellationToken);
        return mapper.ToGameState(game, scoreboard);
    }
}
