using FluentValidation;
using TicTacToe.Application.Queries;

namespace TicTacToe.Application.Validators;

public sealed class GetGameQueryValidator : AbstractValidator<GetGameQuery>
{
    public GetGameQueryValidator() => RuleFor(x => x.GameId).NotEmpty();
}
