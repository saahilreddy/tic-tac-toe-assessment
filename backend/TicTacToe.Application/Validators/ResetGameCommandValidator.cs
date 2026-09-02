using FluentValidation;
using TicTacToe.Application.Commands;

namespace TicTacToe.Application.Validators;

public sealed class ResetGameCommandValidator : AbstractValidator<ResetGameCommand>
{
    public ResetGameCommandValidator()
    {
        RuleFor(x => x.GameId).NotEmpty();
        RuleFor(x => x.ExpectedVersion).GreaterThanOrEqualTo(0);
    }
}
