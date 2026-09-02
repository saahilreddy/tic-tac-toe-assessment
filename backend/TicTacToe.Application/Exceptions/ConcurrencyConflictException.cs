namespace TicTacToe.Application.Exceptions;

public sealed class ConcurrencyConflictException(string message) : Exception(message);
