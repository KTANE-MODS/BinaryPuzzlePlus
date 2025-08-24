using System;

public enum EdgeState
{
    None,
    X,
    Equals
}

public class Edge
{
    public EdgeState State { get; set; } = EdgeState.None;

    public Cell CellA { get; }
    public Cell CellB { get; }

    public Edge(Cell a, Cell b)
    {
        CellA = a;
        CellB = b;
    }

    public Cell GetOther(Cell cell)
    {
        if (cell == CellA) return CellB;
        if (cell == CellB) return CellA;
        throw new ArgumentException("Cell is not part of this edge.");
    }

    public string Log(bool horizontal = true)
    {
        switch (State)
        { 
            case EdgeState.X:
                return "X";
            case EdgeState.Equals:
                return horizontal ? "=" : "॥";
            case EdgeState.None:
                return ".";
        }

        return "";
    }
}