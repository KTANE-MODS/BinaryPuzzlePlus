using NUnit.Framework;
using System.Collections.Generic;

public class Grid
{

    public int Size { get; }
    public Cell[,] Cells { get; }
    public List<Cell> CellList { get; }
    public List<Edge> Edges { get; }

    public Grid(int size)
    {
        Size = size;
        Cells = new Cell[size, size];
        Edges = new List<Edge>();
        CellList = new List<Cell>();

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
                CellList.Add(cell);

                if (r > 0)
                {
                    var neighbor = Cells[r - 1, c];
                    var edge = new Edge(cell, neighbor);
                    Edges.Add(edge);

                    cell.CellUp = neighbor;
                    cell.EdgeUp = edge;

                    neighbor.CellDown = cell;
                    neighbor.EdgeDown = edge;
                }

                if (c > 0)
                {
                    var neighbor = Cells[r, c - 1];
                    var edge = new Edge(cell, neighbor);
                    Edges.Add(edge);

                    cell.CellLeft = neighbor;
                    cell.EdgeLeft = edge;

                    neighbor.CellRight = cell;
                    neighbor.EdgeRight = edge;
                }
            }
        }
    }

    public Grid Copy()
    {
        Grid copy = new Grid(Size);

        //Copy cell values
        for (int r = 0; r < Size; r++)
        {
            for (int c = 0; c < Size; c++)
            {
                copy.Cells[r, c].Value = this.Cells[r, c].Value;
            }
        }

        // Copy edge states
        foreach (var edge in this.Edges)
        {
            // Find matching edge in copy by coordinates
            var a = edge.CellA;
            var b = edge.CellB;

            var copyA = copy.Cells[a.Row, a.Col];
            var copyB = copy.Cells[b.Row, b.Col];

            // The grid constructor already created an edge between A and B.
            // Find it in the copy and sync the state.
            var copyEdge = copy.Edges.Find(e =>
                (e.CellA == copyA && e.CellB == copyB) ||
                (e.CellA == copyB && e.CellB == copyA));

            if (copyEdge != null)
                copyEdge.State = edge.State;
        }

        return copy;
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