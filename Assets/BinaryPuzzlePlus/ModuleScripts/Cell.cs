public enum CellState
{
    Empty,
    Zero,
    One
}

public class Cell
{
    public CellState state;

    public Edge Up;
    public Edge Down;
    public Edge Left;
    public Edge Right;
}