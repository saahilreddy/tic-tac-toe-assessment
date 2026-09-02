namespace TicTacToe.Domain;

public sealed class ComputerMoveSelector
{
    public int SelectMove(IReadOnlyList<string> board)
    {
        ArgumentNullException.ThrowIfNull(board);
        if (board.Count != 9)
            throw new ArgumentException("Board must contain exactly 9 cells.", nameof(board));

        var winningMove = FindCompletion(board, Player.O);
        if (winningMove >= 0)
            return winningMove;

        var blockingMove = FindCompletion(board, Player.X);
        if (blockingMove >= 0)
            return blockingMove;

        if (IsEmpty(board, 4))
            return 4;

        foreach (var corner in new[] { 0, 2, 6, 8 })
        {
            if (IsEmpty(board, corner))
                return corner;
        }

        for (var index = 0; index < 9; index++)
        {
            if (IsEmpty(board, index))
                return index;
        }

        return -1;
    }

    private static int FindCompletion(IReadOnlyList<string> board, Player player)
    {
        for (var index = 0; index < 9; index++)
        {
            if (!IsEmpty(board, index))
                continue;

            var candidate = board.ToArray();
            candidate[index] = player.ToString();
            if (GameRules.FindWinner(candidate)?.Player == player)
                return index;
        }

        return -1;
    }

    private static bool IsEmpty(IReadOnlyList<string> board, int index) => board[index] == string.Empty;
}
