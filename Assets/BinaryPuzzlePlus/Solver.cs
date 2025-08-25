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
using UnityEngine.Rendering;
using NUnit.Framework.Internal.Execution;

public class Solver {
    public static bool Solve(Grid grid)
    {
        Grid copyGrid = grid.Copy();
        bool blockThreeRow;
        bool blockThreeCol;

        bool blockCross;
        bool blockEdge;

        bool equalEdgeDown;
        bool equalEdgeUp;
        bool equalEdgeLeft;
        bool equalEdgeRight;

        bool blockOuterRow;
        bool blockOuterCol;

        bool fillRow;
        do
        {
            blockThreeRow = BlockThreeRow(copyGrid);
            blockCross = BlockCross(copyGrid);
            equalEdgeDown = EqualEdgeDown(copyGrid);
            equalEdgeUp = EqualEdgeUp(copyGrid);
            blockThreeCol = BlockThreeCol(copyGrid);
            blockOuterRow = BlockOuterRow(copyGrid);
            blockOuterCol = BlockOuterCol(copyGrid);
            blockEdge = BlockEdge(copyGrid);
            fillRow = FillRow(copyGrid);
        } while (blockThreeRow || blockCross || equalEdgeDown || equalEdgeUp || blockThreeCol || blockOuterRow || blockOuterCol || blockEdge || fillRow);


        Debug.Log(copyGrid.Log());
        return IsSolved(copyGrid);
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

    private static bool BlockOuterRow(Grid grid) => BlockOuter(grid, true);
    private static bool BlockOuterCol(Grid grid) => BlockOuter(grid, false);

    private static bool BlockOuter(Grid grid, bool isRow)
    {
        // this only works if the size is 6
        if (grid.Size != 6)
            return false;

        bool madeDeduction = false;
        int last = grid.Size - 1;

        for (int i = 0; i < grid.Size; i++)
        {
            // Select the outer cells depending on row/col mode
            Cell first = isRow ? grid.Cells[i, 0] : grid.Cells[0, i];
            Cell lastC = isRow ? grid.Cells[i, last] : grid.Cells[last, i];
            Cell second = isRow ? grid.Cells[i, 1] : grid.Cells[1, i];
            Cell fifth = isRow ? grid.Cells[i, last - 1] : grid.Cells[last - 1, i];

            // first/last must be filled and equal
            if (first.Value == null || lastC.Value == null)
                continue;
            if (first.Value != lastC.Value)
                continue;

            // if either second or fifth is empty, fill them
            if (second.Value == null || fifth.Value == null)
            {
                int value = (int)first.Value == 1 ? 0 : 1;
                second.Value = value;
                fifth.Value = value;

                if (isRow)
                {
                    Debug.Log($"BlockOuterRow: Row {i} Col 1 is {value}");
                    Debug.Log($"BlockOuterRow: Row {i} Col {last - 1} is {value}");
                }
                else
                {
                    Debug.Log($"BlockOuterCol: Row 1 Col {i} is {value}");
                    Debug.Log($"BlockOuterCol: Row {last - 1} Col {i} is {value}");
                }

                madeDeduction = true;
            }
        }

        return madeDeduction;
    }

    private static bool BlockCross(Grid grid)
    {
        //if there is a cross edge that has only one cell filled in, fill the other cell with the opposite value
        return BlockEdges(
            grid,
            EdgeState.X,
            (empty, other) => (other.Value == 1 ? 0 : 1), // opposite value
            "BlockCross"
        );
    }

    private static bool BlockEdge(Grid grid)
    {
        //if there is a equals edge that has only one cell filled in, fill the other cell with the same value
        return BlockEdges(
            grid,
            EdgeState.Equals,
            (empty, other) => (int)other.Value, // same value
            "BlockEdge"
        );
    }

    private static bool BlockEdges(Grid grid, EdgeState state, Func<Cell, Cell, int> valueSelector, string label)
    {
        bool madeDeduction = false;
        List<Edge> edges = grid.Edges.Where(e => e.State == state).ToList();

        foreach (Edge edge in edges)
        {
            Cell[] cells = { edge.CellA, edge.CellB };
            if (cells.Count(c => c.Value == null) == 1)
            {
                Cell emptyCell = cells.First(c => c.Value == null);
                Cell otherCell = edge.GetOther(emptyCell);

                emptyCell.Value = valueSelector(emptyCell, otherCell);
                madeDeduction = true;

                Debug.Log($"{label}: Row {emptyCell.Row} Col {emptyCell.Col} is {emptyCell.Value}");
            }
        }

        return madeDeduction;
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
                    Debug.Log($"EqualEdgeDown: Row {bottomCell.Row + 1} Col {bottomCell.Col} is {bottomCell.Value}");
                }
            }
        }

        return equalEdgeDown;
    }

    private static bool EqualEdgeUp(Grid grid)
    {
        int size = grid.Size;
        bool madeDetuction = false;
        //look at the two cells above the current one
        for (int row = 2; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                //check if this cell is filled
                Cell currentCell = grid.Cells[row, col];
                if (currentCell.Value == null)
                {
                    continue;
                }

                Cell topCell = currentCell.CellUp;

                //check to see the other two cells are empty
                Cell[] topCells = new Cell[] { topCell, topCell.CellUp };
                if (topCells.All(c => c.Value != null))
                {
                    continue;
                }

                //check to see if there is an equal edge between the top two
                if (topCell.EdgeUp != null && topCell.EdgeUp.State == EdgeState.Equals)
                {
                    int value = currentCell.Value == 1 ? 0 : 1;
                    topCell.Value = value;
                    topCell.CellUp.Value = value;
                    madeDetuction = true;
                    Debug.Log($"EqualEdgeDown: Row {topCell.Row} Col {topCell.Col} is {topCell.Value}");
                    Debug.Log($"EqualEdgeDown: Row {topCell.Row - 1} Col {topCell.Col} is {topCell.Value}");
                }
            }
        }

        return madeDetuction;
    }

    private static bool FillRow(Grid grid)
    { 
        int size = grid.Size;
        int halfSize = grid.Size / 2;
        bool madeDetuction = false;
        
        for (int row = 0; row < size; row++)
        {
            //count the number of ones and zeros

            List<Cell> relevantCells = grid.CellList.Where(c => c.Row == row).ToList();

            int oneCount = relevantCells.Count(c => c.Value == 1);
            int zeroCount = relevantCells.Count(c => c.Value == 0);

            //if only one of them equals half the size, fill the empty with the other value
            if (oneCount == halfSize ^ zeroCount == halfSize)
            { 
                int val = zeroCount == halfSize ? 1 : 0;
                List<Cell> emptyCells = relevantCells.Where(c => c.Value == null).ToList();

                foreach (Cell c in emptyCells)
                {
                    c.Value = val;
                    Debug.Log($"FillRow: Row {c.Row} Col {c.Col} is {c.Value}");
                    madeDetuction = true;
                }

            }

        }

        return madeDetuction;
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
}
