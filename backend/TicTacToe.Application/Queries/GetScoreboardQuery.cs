using MediatR;
using TicTacToe.Application.Contracts;
using TicTacToe.Application.Interfaces;

namespace TicTacToe.Application.Queries;

public sealed record GetScoreboardQuery : IRequest<ScoreboardModel>;

public sealed class GetScoreboardQueryHandler(
    IScoreboardRepository scoreboardRepository,
    IApplicationMapper mapper)
    : IRequestHandler<GetScoreboardQuery, ScoreboardModel>
{
    public async Task<ScoreboardModel> Handle(GetScoreboardQuery request, CancellationToken cancellationToken) =>
        mapper.ToScoreboard(await scoreboardRepository.GetAsync(cancellationToken));
}
