using TicTacToe.Application.Contracts;
using TicTacToe.Domain;

namespace TicTacToe.Application.Interfaces;

public interface IApplicationMapper
{
    GameStateModel ToGameState(Game game, ScoreboardSnapshot scoreboard);
    ScoreboardModel ToScoreboard(ScoreboardSnapshot scoreboard);
}
