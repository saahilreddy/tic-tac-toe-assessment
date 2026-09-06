using FluentValidation.TestHelper;
using TicTacToe.Application.Commands;
using TicTacToe.Application.Validators;
using TicTacToe.Domain;
using Xunit;

namespace TicTacToe.Tests.Application;

public sealed class ValidatorTests
{
    private readonly SubmitMoveCommandValidator _validator = new();

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(3, 0)]
    [InlineData(0, -1)]
    [InlineData(0, 3)]
    public void Submit_move_rejects_coordinates_outside_board(
        int row,
        int column)
    {
        var command = new SubmitMoveCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Player.X,
            row,
            column,
            0);

        var result = _validator.TestValidate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Submit_move_rejects_negative_version()
    {
        var command = new SubmitMoveCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Player.X,
            0,
            0,
            -1);

        var result = _validator.TestValidate(command);

        Assert.False(result.IsValid);
    }
}
