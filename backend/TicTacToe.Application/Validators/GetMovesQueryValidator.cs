using FluentValidation;
using TicTacToe.Application.Queries;

namespace TicTacToe.Application.Validators;

public sealed class GetMovesQueryValidator : AbstractValidator<GetMovesQuery>
{
    public GetMovesQueryValidator() => RuleFor(x => x.GameId).NotEmpty();
}
