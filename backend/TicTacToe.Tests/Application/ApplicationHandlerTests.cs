using TicTacToe.Application.Commands;
using TicTacToe.Application.Contracts;
using TicTacToe.Application.Exceptions;
using TicTacToe.Application.Interfaces;
using TicTacToe.Application.Mappings;
using TicTacToe.Application.Queries;
using TicTacToe.Domain;
using TicTacToe.Infrastructure.Persistence;
using Xunit;

namespace TicTacToe.Tests.Application;

public sealed class ApplicationHandlerTests
{
    [Fact]
    public async Task Create_game_starts_with_x_and_empty_board()
    {
        var games = new InMemoryGameRepository();
        var scoreboard = new InMemoryScoreboardRepository();
        var mapper = new ApplicationMapper();
        var handler = new CreateGameCommandHandler(games, scoreboard, mapper);

        var result = await handler.Handle(new CreateGameCommand(GameMode.TwoPlayer), CancellationToken.None);

        Assert.Equal(Player.X, result.CurrentPlayer);
        Assert.All(result.Board, cell => Assert.Equal(string.Empty, cell));
        Assert.Equal(GameStatus.InProgress, result.GameStatus);
    }

    [Fact]
    public async Task Scoreboard_updates_once_when_game_is_completed()
    {
        var games = new InMemoryGameRepository();
        var scoreboard = new InMemoryScoreboardRepository();
        var mapper = new ApplicationMapper();
        var create = new CreateGameCommandHandler(games, scoreboard, mapper);
        var submit = new SubmitMoveCommandHandler(games, scoreboard, mapper, new ComputerMoveSelector());
        var game = await create.Handle(new CreateGameCommand(GameMode.TwoPlayer), CancellationToken.None);

        var moves = new (Player Player, int Row, int Column)[]
        {
            (Player.X, 0, 0), (Player.O, 1, 0),
            (Player.X, 0, 1), (Player.O, 1, 1),
            (Player.X, 0, 2)
        };

        foreach (var move in moves)
        {
            var response = await submit.Handle(new SubmitMoveCommand(
                game.GameId, game.GameId, move.Player, move.Row, move.Column, game.Version), CancellationToken.None);
            Assert.True(response.IsSuccess);
            game = response.Value!;
        }

        Assert.Equal(GameStatus.Won, game.GameStatus);
        Assert.Equal(1, game.Scoreboard.XWins);
        Assert.Equal(0, game.Scoreboard.OWins);

        var scoreboardState = await scoreboard.GetAsync();
        Assert.Equal(1, scoreboardState.XWins);
    }

    [Fact]
    public async Task Undo_in_computer_mode_removes_human_and_computer_moves()
    {
        var games = new InMemoryGameRepository();
        var scoreboard = new InMemoryScoreboardRepository();
        var mapper = new ApplicationMapper();
        var create = new CreateGameCommandHandler(games, scoreboard, mapper);
        var submit = new SubmitMoveCommandHandler(games, scoreboard, mapper, new ComputerMoveSelector());
        var undo = new UndoMoveCommandHandler(games, scoreboard, mapper);
        var game = await create.Handle(new CreateGameCommand(GameMode.Computer), CancellationToken.None);

        var afterMove = await submit.Handle(
            new SubmitMoveCommand(game.GameId, game.GameId, Player.X, 0, 0, game.Version), CancellationToken.None);
        Assert.True(afterMove.IsSuccess);
        Assert.Equal(2, afterMove.Value!.MoveHistory.Count);

        var afterUndo = await undo.Handle(
            new UndoMoveCommand(game.GameId, afterMove.Value.Version), CancellationToken.None);

        Assert.True(afterUndo.IsSuccess);
        Assert.Empty(afterUndo.Value!.MoveHistory);
        Assert.Equal(Player.X, afterUndo.Value.CurrentPlayer);
    }

    [Fact]
    public async Task Stale_version_returns_concurrency_conflict()
    {
        var games = new InMemoryGameRepository();
        var scoreboard = new InMemoryScoreboardRepository();
        var mapper = new ApplicationMapper();
        var create = new CreateGameCommandHandler(games, scoreboard, mapper);
        var submit = new SubmitMoveCommandHandler(games, scoreboard, mapper, new ComputerMoveSelector());
        var game = await create.Handle(new CreateGameCommand(GameMode.TwoPlayer), CancellationToken.None);

        var first = await submit.Handle(new SubmitMoveCommand(game.GameId, game.GameId, Player.X, 0, 0, 0), CancellationToken.None);
        Assert.True(first.IsSuccess);

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => submit.Handle(
            new SubmitMoveCommand(game.GameId, game.GameId, Player.O, 1, 1, 0), CancellationToken.None));
    }
}
