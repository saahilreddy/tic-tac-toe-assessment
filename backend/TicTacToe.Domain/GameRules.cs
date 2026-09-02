namespace TicTacToe.Domain;

public static class GameRules
{
    private static readonly int[][] WinningLines =
    [
        [0, 1, 2], [3, 4, 5], [6, 7, 8],
        [0, 3, 6], [1, 4, 7], [2, 5, 8],
        [0, 4, 8], [2, 4, 6]
    ];

    public static RuleValidationResult ValidateMove(Game game, Move move)
    {
        if (game.IsCompleted)
            return RuleValidationResult.Invalid("The game is already completed.");

        if (move.Row is < 0 or > 2 || move.Column is < 0 or > 2)
            return RuleValidationResult.Invalid("Row and column must each be between 0 and 2.");

        if (move.Player != game.CurrentPlayer)
            return RuleValidationResult.Invalid($"It is {game.CurrentPlayer}'s turn.");

        if (game.Mode == GameMode.Computer && move.Player != Player.X)
            return RuleValidationResult.Invalid("In Computer Mode, the human player is X.");

        if (game.Board[move.CellIndex] != string.Empty)
            return RuleValidationResult.Invalid("The selected cell is already occupied.");

        return RuleValidationResult.Valid();
    }

    public static (Player Player, IReadOnlyList<int> Cells)? FindWinner(IReadOnlyList<string> board)
    {
        ArgumentNullException.ThrowIfNull(board);
        if (board.Count != 9)
            throw new ArgumentException("Board must contain exactly 9 cells.", nameof(board));

        foreach (var cells in WinningLines)
        {
            var value = board[cells[0]];
            if (value != string.Empty && value == board[cells[1]] && value == board[cells[2]])
            {
                var player = value == Player.X.ToString() ? Player.X : Player.O;
                return (player, cells);
            }
        }

        return null;
    }

    public static GameOutcome EvaluateOutcome(Game game)
    {
        var winner = FindWinner(game.Board);
        if (winner is not null)
            return winner.Value.Player == Player.X ? GameOutcome.XWon : GameOutcome.OWon;

        return game.Board.All(cell => cell != string.Empty)
            ? GameOutcome.Draw
            : GameOutcome.InProgress;
    }
}
