namespace TicTacToe.Domain;

public enum Player
{
    X,
    O
}

public enum GameMode
{
    TwoPlayer,
    Computer
}

public enum GameStatus
{
    InProgress,
    Won,
    Draw
}

public enum GameOutcome
{
    InProgress,
    XWon,
    OWon,
    Draw
}
