namespace TicTacToe.Domain;

public sealed record Move(int MoveNumber, Player Player, int Row, int Column)
{
    public int CellIndex => Row * 3 + Column;
}
