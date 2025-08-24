using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using KModkit;
using Rnd = UnityEngine.Random;
using static HarmonyLib.Code;
using UnityEngine.Experimental.UIElements;

public class Solver {
    public static bool Solve(Grid grid)
    {
        Grid copyGrid = grid.Copy();
        bool blockThreeRow;
        bool blockCross;
        bool equalEdgeDown;
        bool blockThreeCol;

        do
        {
            blockThreeRow = BlockThreeRow(copyGrid);
            blockCross = BlockCross(copyGrid);
            equalEdgeDown = EqualEdgeDown(copyGrid);
            blockThreeCol = BlockThreeCol(copyGrid);
        } while (blockThreeRow || blockCross || equalEdgeDown || blockThreeCol);


        Debug.Log(copyGrid.Log());
        return IsSolved(copyGrid);
    }

    public static bool IsSolved(Grid grid)
    {
        int size = grid.Size;
        //check that all the cells are filled
        if (!grid.CellList.All(c => c.Value != null))
        {
            return false;
        }

        // Check that we don’t get more than two of the same digit in a straight row/column
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                if (row >= 2 && grid.Cells[row, col].Value == grid.Cells[row - 1, col].Value && grid.Cells[row, col].Value == grid.Cells[row - 2, col].Value)
                {
                    return false;
                }

                if (col >= 2 && grid.Cells[row, col].Value == grid.Cells[row, col - 1].Value && grid.Cells[row, col].Value == grid.Cells[row, col - 2].Value)
                {
                    return false;
                }
            }
        }

        //For all edges that that are X's, make sure the cells are opposite
        if (!ValidateEdges(grid.Edges.Where(e => e.State == EdgeState.X),
                           (a, b) => a != b))
        {
            return false;
        }

        //For all edges that that are ='s, make sure the cells are the same
        if (!ValidateEdges(grid.Edges.Where(e => e.State == EdgeState.Equals),
                           (a, b) => a == b))
        {
            return false;
        }

        return true;
    }

    private static bool ValidateEdges(IEnumerable<Edge> edges, Func<int, int, bool> condition)
    {
        foreach (var edge in edges)
        {
            if (!condition((int)edge.CellA.Value, (int)edge.CellB.Value))
            {
                return false;
            }
        }
        return true;
    }


    private static bool BlockThreeRow(Grid grid)
    {
        int size = grid.Size;

        var triplets = from row in Enumerable.Range(0, size)
                       from col in Enumerable.Range(0, size - 2)
                       select new Cell[]
                       {
                       grid.Cells[row, col],
                       grid.Cells[row, col + 1],
                       grid.Cells[row, col + 2]
                       };

        return BlockThree(triplets, "BlockThreeRow");
    }

    private static bool BlockThreeCol(Grid grid)
    {
        int size = grid.Size;

        var triplets = from row in Enumerable.Range(0, size - 2)
                       from col in Enumerable.Range(0, size)
                       select new Cell[]
                       {
                       grid.Cells[row, col],
                       grid.Cells[row + 1, col],
                       grid.Cells[row + 2, col]
                       };

        return BlockThree(triplets, "BlockThreeCol");
    }

    private static bool BlockThree(IEnumerable<Cell[]> triplets, string label)
    {
        bool madeDeduction = false;

        foreach (var cells in triplets)
        {
            // Only one cell should be empty
            if (cells.Count(c => c.Value == null) != 1)
                continue;

            var emptyCell = cells.First(c => c.Value == null);
            var nonEmpty = cells.Where(c => c.Value != null).ToArray();

            // If the other two are the same, block the third
            if (nonEmpty[0].Value == nonEmpty[1].Value)
            {
                emptyCell.Value = nonEmpty[0].Value == 1 ? 0 : 1;
                Debug.Log($"{label}: Row {emptyCell.Row} Col {emptyCell.Col} is {emptyCell.Value}");
                madeDeduction = true;
            }
        }

        return madeDeduction;
    }


    private static bool BlockCross(Grid grid)
    {
        //if there is a cross edge that has at least one cell filled in, fill the other cell with the opposite value
        bool blockCross = false;
        List<Edge> crossEdges = grid.Edges.Where(e => e.State == EdgeState.X).ToList();

        foreach (Edge edge in crossEdges)
        {
            Cell[] cells = new Cell[] { edge.CellA, edge.CellB };
            if (cells.Count(c => c.Value == null) == 1)
            {
                Cell emptyCell = cells.First(c => c.Value == null);
                emptyCell.Value = edge.GetOther(emptyCell).Value == 1 ? 0 : 1;
                blockCross = true;
                Debug.Log($"BlockCross: Row {emptyCell.Row} Col {emptyCell.Col} is {emptyCell.Value}");
            }
        }

        return blockCross;
    }

    private static bool EqualEdgeDown(Grid grid)
    { 
        int size = grid.Size;
        bool equalEdgeDown = false;
        //look at the two cells below from the current one
        for (int row = 0; row < size - 2; row++)
        {
            for (int col = 0; col < size; col++)
            {
                //check if this cell is filled
                Cell currentCell = grid.Cells[row, col];
                if (currentCell.Value == null)
                {
                    continue;
                }

                Cell bottomCell = currentCell.CellDown;

                //check to see the other two cells are empty
                Cell[] bottomCells = new Cell[] { bottomCell, bottomCell.CellDown };
                if (bottomCells.All(c => c.Value != null))
                {
                    continue;
                }

                //check to see if there is an equal edge between the bottom two
                if (bottomCell.EdgeDown != null && bottomCell.EdgeDown.State == EdgeState.Equals)
                {
                    int value = currentCell.Value == 1 ? 0 : 1;
                    bottomCell.Value = value;
                    bottomCell.CellDown.Value = value;
                    equalEdgeDown = true;
                    Debug.Log($"EqualEdgeDown: Row {bottomCell.Row} Col {bottomCell.Col} is {bottomCell.Value}");
                    Debug.Log($"EqualEdgeDown: Row {bottomCell.Row} Col {bottomCell.Col + 1} is {bottomCell.Value}");
                }
            }
        }

        return equalEdgeDown;
    }
}
