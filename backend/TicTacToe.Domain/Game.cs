namespace TicTacToe.Domain;

public sealed class Game
{
    private readonly string[] _board;
    private readonly List<Move> _moves;
    private readonly List<int> _winningCells;

    public Guid Id { get; }
    public GameMode Mode { get; }
    public long Version { get; private set; }
    public Player CurrentPlayer { get; private set; }
    public GameStatus Status { get; private set; }
    public Player? Winner { get; private set; }
    public bool IsCompleted => Status != GameStatus.InProgress;
    public IReadOnlyList<string> Board => _board;
    public IReadOnlyList<Move> Moves => _moves;
    public IReadOnlyList<int> WinningCells => _winningCells;

    public Game(Guid id, GameMode mode)
    {
        Id = id;
        Mode = mode;
        _board = Enumerable.Repeat(string.Empty, 9).ToArray();
        _moves = [];
        _winningCells = [];
        CurrentPlayer = Player.X;
        Status = GameStatus.InProgress;
        Version = 0;
    }

    private Game(
        Guid id,
        GameMode mode,
        long version,
        Player currentPlayer,
        GameStatus status,
        Player? winner,
        string[] board,
        List<Move> moves,
        List<int> winningCells)
    {
        Id = id;
        Mode = mode;
        Version = version;
        CurrentPlayer = currentPlayer;
        Status = status;
        Winner = winner;
        _board = board;
        _moves = moves;
        _winningCells = winningCells;
    }

    public void ApplyMove(Move move)
    {
        if (_board[move.CellIndex] != string.Empty)
            throw new DomainInvariantException("A validated move cannot overwrite an occupied cell.");

        _board[move.CellIndex] = move.Player.ToString();
        _moves.Add(move);
        CurrentPlayer = move.Player == Player.X ? Player.O : Player.X;
        Version++;
    }

    public void CompleteWin(Player winner, IReadOnlyList<int> cells)
    {
        if (cells.Count != 3)
            throw new DomainInvariantException("A winning line must contain exactly three cells.");

        Status = GameStatus.Won;
        Winner = winner;
        _winningCells.Clear();
        _winningCells.AddRange(cells);
        Version++;
    }

    public void CompleteDraw()
    {
        Status = GameStatus.Draw;
        Winner = null;
        _winningCells.Clear();
        Version++;
    }

    public void UndoLastMoves(int numberOfMoves)
    {
        if (numberOfMoves <= 0 || numberOfMoves > _moves.Count)
            throw new DomainInvariantException("The requested number of moves cannot be undone.");

        _moves.RemoveRange(_moves.Count - numberOfMoves, numberOfMoves);
        RebuildFromMoves();
        Version++;
    }

    public Game Clone() => new(
        Id,
        Mode,
        Version,
        CurrentPlayer,
        Status,
        Winner,
        _board.ToArray(),
        _moves.ToList(),
        _winningCells.ToList());

    private void RebuildFromMoves()
    {
        Array.Fill(_board, string.Empty);
        _winningCells.Clear();
        Winner = null;
        Status = GameStatus.InProgress;
        CurrentPlayer = Player.X;

        for (var index = 0; index < _moves.Count; index++)
        {
            var move = _moves[index];
            _board[move.CellIndex] = move.Player.ToString();
            CurrentPlayer = move.Player == Player.X ? Player.O : Player.X;
            _moves[index] = move with { MoveNumber = index + 1 };
        }
    }
}
