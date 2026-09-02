using TicTacToe.Domain;
using Xunit;

namespace TicTacToe.Tests.Domain;

public sealed class ComputerMoveSelectorTests
{
    private readonly ComputerMoveSelector _selector = new();

    [Fact]
    public void Takes_winning_move_first()
    {
        var board = new[] { "O", "O", "", "X", "X", "", "", "", "" };
        Assert.Equal(2, _selector.SelectMove(board));
    }

    [Fact]
    public void Blocks_x_before_center()
    {
        var board = new[] { "X", "X", "", "O", "", "", "", "", "" };
        Assert.Equal(2, _selector.SelectMove(board));
    }

    [Fact]
    public void Takes_center_when_available()
    {
        var board = new[] { "X", "", "", "", "", "", "", "", "" };
        Assert.Equal(4, _selector.SelectMove(board));
    }

    [Fact]
    public void Takes_first_available_corner_after_center_is_used()
    {
        var board = new[] { "X", "O", "", "", "X", "", "", "", "O" };
        Assert.Equal(2, _selector.SelectMove(board));
    }

    [Fact]
    public void Takes_any_remaining_cell_when_no_corner_exists()
    {
        var board = new[] { "X", "O", "X", "O", "X", "O", "O", "X", "" };
        Assert.Equal(8, _selector.SelectMove(board));
    }
}
