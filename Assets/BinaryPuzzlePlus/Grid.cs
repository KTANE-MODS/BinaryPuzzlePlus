public class Grid
{

    public int Size { get; }
    public Cell[,] Cells { get; }

    public Grid(int size)
    {
        Size = size;
        Cells = new Cell[size, size];

        // Create all cells
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                Cells[r, c] = new Cell(r, c);

        // Connect cells + edges
        for (int r = 0; r < Size; r++)
        {
            for (int c = 0; c < Size; c++)
            {
                var cell = Cells[r, c];

                if (r > 0)
                {
                    var neighbor = Cells[r - 1, c];
                    var edge = new Edge(cell, neighbor);

                    cell.CellUp = neighbor;
                    cell.EdgeUp = edge;

                    neighbor.CellDown = cell;
                    neighbor.EdgeDown = edge;
                }

                if (c > 0)
                {
                    var neighbor = Cells[r, c - 1];
                    var edge = new Edge(cell, neighbor);

                    cell.CellLeft = neighbor;
                    cell.EdgeLeft = edge;

                    neighbor.CellRight = cell;
                    neighbor.EdgeRight = edge;
                }
            }
        }
    }

    public string Log()
    {
        string s = "\n";
        for (int row = 0; row < Size; row++)
        {
            s += LogCellRow(row);
            s += LogEdgeRow(row);
        }
        return s;
    }

    private string LogCellRow(int row)
    {
        string s = "";
        for (int col = 0; col < Size; col++)
        {
            Cell c = Cells[row, col];
            s += c.Log() + " " + $"{(c.EdgeRight != null ? c.EdgeRight.Log() : "")}" + " ";
        }

        return s.Trim() + "\n";
    }

    private string LogEdgeRow(int row)
    {
        string s = "";
        for (int col = 0; col < Size; col++)
        {
            Edge e = Cells[row, col].EdgeDown;
            if (e == null)
            {
                break;
            }

            s += $"{e.Log(false)}   ";
        }

        return s.Trim() + "\n";
    }
}