using FluentValidation;
using TicTacToe.Application.Commands;

namespace TicTacToe.Application.Validators;

public sealed class UndoMoveCommandValidator : AbstractValidator<UndoMoveCommand>
{
    public UndoMoveCommandValidator()
    {
        RuleFor(x => x.GameId).NotEmpty();
        RuleFor(x => x.ExpectedVersion).GreaterThanOrEqualTo(0);
    }
}
