using TicTacToe.Domain;
using Xunit;

namespace TicTacToe.Tests.Domain;

public sealed class GameRulesTests
{
    [Fact]
    public void Valid_move_is_accepted()
    {
        var game = new Game(Guid.NewGuid(), GameMode.TwoPlayer);
        var result = GameRules.ValidateMove(game, new Move(1, Player.X, 0, 0));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Occupied_cell_is_rejected()
    {
        var game = new Game(Guid.NewGuid(), GameMode.TwoPlayer);
        game.ApplyMove(new Move(1, Player.X, 0, 0));
        var result = GameRules.ValidateMove(game, new Move(2, Player.O, 0, 0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Wrong_player_turn_is_rejected()
    {
        var game = new Game(Guid.NewGuid(), GameMode.TwoPlayer);
        var result = GameRules.ValidateMove(game, new Move(1, Player.O, 0, 0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Row_win_is_detected()
    {
        var board = new[] { "X", "X", "X", "", "", "", "", "", "" };
        var winner = GameRules.FindWinner(board);
        Assert.Equal(Player.X, winner!.Value.Player);
        Assert.Equal([0, 1, 2], winner.Value.Cells);
    }

    [Fact]
    public void Column_win_is_detected()
    {
        var board = new[] { "O", "", "", "O", "", "", "O", "", "" };
        var winner = GameRules.FindWinner(board);
        Assert.Equal(Player.O, winner!.Value.Player);
        Assert.Equal([0, 3, 6], winner.Value.Cells);
    }

    [Fact]
    public void Diagonal_win_is_detected()
    {
        var board = new[] { "X", "", "", "", "X", "", "", "", "X" };
        var winner = GameRules.FindWinner(board);
        Assert.Equal(Player.X, winner!.Value.Player);
        Assert.Equal([0, 4, 8], winner.Value.Cells);
    }

    [Fact]
    public void Full_board_without_winner_is_draw()
    {
        var board = new[] { "X", "O", "X", "X", "O", "O", "O", "X", "X" };
        var game = new Game(Guid.NewGuid(), GameMode.TwoPlayer);
        for (var i = 0; i < board.Length; i++)
        {
            if (board[i] == string.Empty) continue;
            var player = board[i] == "X" ? Player.X : Player.O;
            game.ApplyMove(new Move(i + 1, player, i / 3, i % 3));
        }

        Assert.Equal(GameOutcome.Draw, GameRules.EvaluateOutcome(game));
    }

    [Fact]
    public void Moves_after_completion_are_rejected()
    {
        var game = new Game(Guid.NewGuid(), GameMode.TwoPlayer);
        game.ApplyMove(new Move(1, Player.X, 0, 0));
        game.ApplyMove(new Move(2, Player.O, 1, 0));
        game.ApplyMove(new Move(3, Player.X, 0, 1));
        game.ApplyMove(new Move(4, Player.O, 1, 1));
        game.ApplyMove(new Move(5, Player.X, 0, 2));
        game.CompleteWin(Player.X, [0, 1, 2]);

        var result = GameRules.ValidateMove(game, new Move(6, Player.O, 2, 2));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Undo_restores_previous_two_player_state()
    {
        var game = new Game(Guid.NewGuid(), GameMode.TwoPlayer);
        game.ApplyMove(new Move(1, Player.X, 0, 0));
        game.ApplyMove(new Move(2, Player.O, 1, 1));
        game.UndoLastMoves(1);

        Assert.Equal(Player.O, game.CurrentPlayer);
        Assert.Empty(game.Board[4]);
        Assert.Single(game.Moves);
    }
}
