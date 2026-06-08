using System.Collections.Generic;
using System.Text;

public class PuzzleState
{
    public int Size { get; }

    public Cell[] Cells { get; }

    public List<Edge> Edges { get; }

    public PuzzleState(int size)
    {
        Size = size;
        Cells = new Cell[size * size];
        Edges = new List<Edge>();

        CreateCells();
        CreateEdges();
    }

    private void CreateCells()
    {
        for (int i = 0; i < Cells.Length; i++)
        {
            Cells[i] = new Cell();
        }
    }

    private void CreateEdges()
    {
        for (int i = 0; i < Cells.Length; i++)
        {
            int row = i / Size;
            int col = i % Size;
            Cell current = Cells[i];

            // Right edge
            if (col < Size - 1)
            {
                Cell right = Cells[i + 1];

                Edge edge = new Edge
                {
                    CellA = current,
                    CellB = right
                };
                Edges.Add(edge);

                current.Right = edge;
                right.Left = edge;
            }

            // Down edge
            if (row < Size - 1)
            {
                Cell down = Cells[i + Size];

                Edge edge = new Edge
                {
                    CellA = current,
                    CellB = down
                };
                Edges.Add(edge);

                current.Down = edge;
                down.Up = edge;
            }
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        for (int row = 0; row < Size; row++)
        {
            // Print cells and horizontal edges
            for (int col = 0; col < Size; col++)
            {
                Cell cell = Cells[row * Size + col];

                sb.Append(CellToString(cell.state));

                if (col < Size - 1)
                {
                    sb.Append(' ');
                    sb.Append(EdgeToString(cell.Right?.Constraint ?? EdgeState.None));
                    sb.Append(' ');
                }
            }

            sb.AppendLine();

            // Print vertical edges
            if (row < Size - 1)
            {
                for (int col = 0; col < Size; col++)
                {
                    Cell cell = Cells[row * Size + col];

                    sb.Append(EdgeToString(cell.Down?.Constraint ?? EdgeState.None));

                    if (col < Size - 1)
                        sb.Append("   ");
                }

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static string CellToString(CellState state)
    {
        switch(state)
        {
            case CellState.Zero:
                return "0";
            case CellState.One:
                return "1";
            default:
                return ".";
        }
    }

    private static string EdgeToString(EdgeState edge)
    {
        switch(edge)
        {
            case EdgeState.Equal:
                return "=";
            case EdgeState.X:
                return "X";
            default:
                return ".";
        }
    }
}