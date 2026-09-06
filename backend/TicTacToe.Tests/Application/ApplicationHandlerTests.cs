using Moq;
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
    public async Task Scoreboard_applies_same_game_result_only_once()
    {
        var scoreboard = new InMemoryScoreboardRepository();
        var gameId = Guid.NewGuid();

        await scoreboard.ApplyGameResultOnceAsync(
            gameId,
            GameOutcome.XWon);

        await scoreboard.ApplyGameResultOnceAsync(
            gameId,
            GameOutcome.XWon);

        var result = await scoreboard.GetAsync();

        Assert.Equal(1, result.XWins);
        Assert.Equal(0, result.OWins);
        Assert.Equal(0, result.Draws);
    }
    [Fact]
    public async Task Submit_move_returns_not_found_for_unknown_game()
    {
        var games = new InMemoryGameRepository();
        var scoreboard = new InMemoryScoreboardRepository();
        var mapper = new ApplicationMapper();

        var handler = new SubmitMoveCommandHandler(
            games,
            scoreboard,
            mapper,
            new ComputerMoveSelector());
        var guid = Guid.NewGuid();
        var result = await handler.Handle(
            new SubmitMoveCommand(
                guid,
                guid,
                Player.X,
                0,
                0,
                0),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
    [Fact]
    public async Task Submit_move_throws_concurrency_exception_when_update_fails()
    {
        var games = new Mock<IGameRepository>();
        var scoreboard = new Mock<IScoreboardRepository>();
        var mapper = new ApplicationMapper();

        var game = new Game(
            Guid.NewGuid(),
            GameMode.TwoPlayer);

        games
            .Setup(x => x.GetAsync(
                game.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        games
            .Setup(x => x.TryUpdateAsync(
                It.IsAny<Game>(),
                0,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new SubmitMoveCommandHandler(
            games.Object,
            scoreboard.Object,
            mapper,
            new ComputerMoveSelector());

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() =>
            handler.Handle(
                new SubmitMoveCommand(
                    game.Id,
                    game.Id,
                    Player.X,
                    0,
                    0,
                    0),
                CancellationToken.None));
    }
    [Fact]
    public async Task Submit_move_rejects_mismatched_game_ids()
    {
        var games = new InMemoryGameRepository();
        var scoreboard = new InMemoryScoreboardRepository();
        var mapper = new ApplicationMapper();

        var handler = new SubmitMoveCommandHandler(
            games,
            scoreboard,
            mapper,
            new ComputerMoveSelector());

        var result = await handler.Handle(
            new SubmitMoveCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Player.X,
                0,
                0,
                0),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
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
    [Fact]
    public async Task Undo_in_two_player_mode_removes_only_last_move()
    {
        var games = new InMemoryGameRepository();
        var scoreboard = new InMemoryScoreboardRepository();
        var mapper = new ApplicationMapper();

        var create = new CreateGameCommandHandler(
            games,
            scoreboard,
            mapper);

        var submit = new SubmitMoveCommandHandler(
            games,
            scoreboard,
            mapper,
            new ComputerMoveSelector());

        var undo = new UndoMoveCommandHandler(
            games,
            scoreboard,
            mapper);

        var game = await create.Handle(
            new CreateGameCommand(GameMode.TwoPlayer),
            CancellationToken.None);

        var first = await submit.Handle(
            new SubmitMoveCommand(
                game.GameId,
                game.GameId,
                Player.X,
                0,
                0,
                game.Version),
            CancellationToken.None);

        game = first.Value!;

        var second = await submit.Handle(
            new SubmitMoveCommand(
                game.GameId,
                game.GameId,
                Player.O,
                1,
                1,
                game.Version),
            CancellationToken.None);

        game = second.Value!;

        var result = await undo.Handle(
            new UndoMoveCommand(
                game.GameId,
                game.Version),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var state = result.Value!;

        Assert.Single(state.MoveHistory);
        Assert.Equal(Player.X, state.MoveHistory[0].Player);
        Assert.Equal(0, state.MoveHistory[0].CellIndex);
        Assert.Equal(Player.O, state.CurrentPlayer);
        Assert.Equal(string.Empty, state.Board[4]);
    }
    [Fact]
    public async Task Reset_game_creates_fresh_game_and_keeps_scoreboard()
    {
        var games = new InMemoryGameRepository();
        var scoreboard = new InMemoryScoreboardRepository();
        var mapper = new ApplicationMapper();

        var create = new CreateGameCommandHandler(
            games,
            scoreboard,
            mapper);

        var reset = new ResetGameCommandHandler(
            games,
            scoreboard,
            mapper);

        var game = await create.Handle(
            new CreateGameCommand(GameMode.TwoPlayer),
            CancellationToken.None);

        var oldGameId = game.GameId;

        var result = await reset.Handle(
            new ResetGameCommand(
                game.GameId,
                game.Version),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var newGame = result.Value!;

        Assert.NotEqual(oldGameId, newGame.GameId);
        Assert.Equal(Player.X, newGame.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, newGame.GameStatus);
        Assert.Null(newGame.Winner);
        Assert.Empty(newGame.WinningCells);
        Assert.Empty(newGame.MoveHistory);
        Assert.All(
            newGame.Board,
            cell => Assert.Equal(string.Empty, cell));
    }
    [Fact]
    public async Task Reset_scoreboard_clears_all_scores()
    {
        var scoreboard = new InMemoryScoreboardRepository();
        var mapper = new ApplicationMapper();

        var handler = new ResetScoreboardCommandHandler(
            scoreboard,
            mapper);

        await scoreboard.ApplyGameResultOnceAsync(
            Guid.NewGuid(),
            GameOutcome.XWon);

        var before = await scoreboard.GetAsync();

        Assert.Equal(1, before.XWins);

        var result = await handler.Handle(
            new ResetScoreboardCommand(),
            CancellationToken.None);

        Assert.Equal(0, result.XWins);
        Assert.Equal(0, result.OWins);
        Assert.Equal(0, result.Draws);
    }
}
