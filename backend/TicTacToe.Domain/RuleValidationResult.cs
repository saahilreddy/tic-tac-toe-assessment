namespace TicTacToe.Domain;

public sealed record RuleValidationResult(bool IsValid, string? ErrorMessage)
{
    public static RuleValidationResult Valid() => new(true, null);

    public static RuleValidationResult Invalid(string message) => new(false, message);
}
