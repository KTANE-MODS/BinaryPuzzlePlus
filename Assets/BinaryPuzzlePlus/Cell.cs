public class Cell
{
    public int Row { get; }
    public int Col { get; }
    public int? Value { get; set; } // null = empty, 0 (black) or 1 = filled (white)
    public bool Permanent { get; set; } //if this can be modified by the user

    // Neighbor cells
    public Cell CellUp { get; set; }
    public Cell CellDown { get; set; }
    public Cell CellLeft { get; set; }
    public Cell CellRight { get; set; }

    // Neighbor edges
    public Edge EdgeUp { get; set; }
    public Edge EdgeDown { get; set; }
    public Edge EdgeLeft { get; set; }
    public Edge EdgeRight { get; set; }

    public Cell(int row, int col)
    {
        Row = row;
        Col = col;
        Permanent = false;
    }

    public string Log()
    { 
        return Value == null ? "-" : Value.ToString();
    }
}