using MediatR;
using TicTacToe.Application.Contracts;
using TicTacToe.Application.Interfaces;

namespace TicTacToe.Application.Commands;

public sealed record ResetScoreboardCommand : IRequest<ScoreboardModel>;

public sealed class ResetScoreboardCommandHandler(
    IScoreboardRepository scoreboardRepository,
    IApplicationMapper mapper)
    : IRequestHandler<ResetScoreboardCommand, ScoreboardModel>
{
    public async Task<ScoreboardModel> Handle(ResetScoreboardCommand request, CancellationToken cancellationToken)
    {
        await scoreboardRepository.ResetAsync(cancellationToken);
        return mapper.ToScoreboard(await scoreboardRepository.GetAsync(cancellationToken));
    }
}
