namespace TicTacToe.Domain;

public sealed class DomainInvariantException(string message) : Exception(message);
