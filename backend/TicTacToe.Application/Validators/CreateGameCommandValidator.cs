using FluentValidation;
using TicTacToe.Application.Commands;

namespace TicTacToe.Application.Validators;

public sealed class CreateGameCommandValidator : AbstractValidator<CreateGameCommand>
{
    public CreateGameCommandValidator()
    {
        RuleFor(x => x.Mode).IsInEnum();
    }
}
