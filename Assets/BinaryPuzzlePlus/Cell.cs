using UnityEngine;

public class Cell
{
    public int Row { get; }
    public int Col { get; }
    public int? Value { get; set; } // null = empty, 0 (black) or 1 = filled (white)
    public bool Permanent { get; set; } //if this can be modified by the user
    public TextMesh Text { get; set; }

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

    public void Interact()
    {
        //if permanent, don't do anything
        if (Permanent)
        {
            return;
        }

        //otherwise cycle through values: none, 0, 1
        if (Value == null)
        {
            Value = 0;
        }

        else if (Value == 0)
        {
            Value = 1;
        }

        else
        {
            Value = null;
        }

        UpdateText();
    }

    public void UpdateText()
    {
        Text.text = GetCellString();
    }

    public string Log()
    {
        return GetCellString();
    }

    private string GetCellString()
    {
        return Value == null ? "-" : Value.ToString();
    }
}