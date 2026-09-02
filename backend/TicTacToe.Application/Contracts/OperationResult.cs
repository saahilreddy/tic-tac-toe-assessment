namespace TicTacToe.Application.Contracts;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict
}

public sealed record ApplicationError(ErrorType Type, string Message);

public sealed class OperationResult<T>
{
    private OperationResult(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private OperationResult(ApplicationError error)
    {
        IsSuccess = false;
        Error = error;
    }

    public bool IsSuccess { get; }
    public T? Value { get; }
    public ApplicationError? Error { get; }

    public static OperationResult<T> Success(T value) => new(value);

    public static OperationResult<T> Failure(ErrorType type, string message) =>
        new(new ApplicationError(type, message));
}
