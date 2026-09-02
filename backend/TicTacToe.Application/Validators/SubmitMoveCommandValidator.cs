using FluentValidation;
using TicTacToe.Application.Commands;

namespace TicTacToe.Application.Validators;

public sealed class SubmitMoveCommandValidator : AbstractValidator<SubmitMoveCommand>
{
    public SubmitMoveCommandValidator()
    {
        RuleFor(x => x.GameId).NotEmpty();
        RuleFor(x => x.RequestGameId).NotEmpty();
        RuleFor(x => x.Player).IsInEnum();
        RuleFor(x => x.Row).InclusiveBetween(0, 2);
        RuleFor(x => x.Column).InclusiveBetween(0, 2);
        RuleFor(x => x.ExpectedVersion).GreaterThanOrEqualTo(0);
    }
}
