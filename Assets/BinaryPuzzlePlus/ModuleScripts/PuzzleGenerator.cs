using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Rnd = UnityEngine.Random;
public static class PuzzleGenerator {

    //if true, the generator will generate cells and edges at the same time. Othwerwise, it will generate them separatley.
    public static bool LongGeneration = false;

    //the width and length of the grid
    public static int Size;

    //If rows/columns should be distinct from one another
    public static bool DistinctRows;

    //The chance of a constraint being removed when generating the puzzle. Used when LongGeneration is false. This is done after the solution is generated to reduce puzzle generation time. Used to vary the difficulty of the puzzle. 
    public static int ConstraintRemovalPercentage;

    /// <summary>
    /// Generate the puzzle solution.
    /// </summary>
    /// <returns>The generated puzzle</returns>
    public static PuzzleState GeneratePuzzle()
    {
        //if LongGeneration is true, Generate the cells first, then edges afterwards to reduce the time it takes to
        //generate a puzzle. Otherwise, generate the cells and edges at the same time. This makes the space to
        //search for a puzzle 2^(size*size) * 3^(2*r*c-r-c) before pruning. But it's a more authentic generation
        //method since the constraints are added at the same time as the cells are generated.
        PuzzleState solution = new PuzzleState(Size);

        solution = GeneratePuzzleIndex(solution, new bool[Size * Size], new bool[Size * Size], new bool[Size * Size], 0).First();
        if (!LongGeneration)
        { 
            AddConstraints(solution);
        }
        return solution;
    }

    /// <summary>
    /// Generates a random puzzle solution
    /// </summary>
    /// <param name="current">The current generated puzzle</param>
    /// <param name="cellStateGiven">An array that tells which cells are forced into the puzzle</param>
    /// <param name="leftEdgeGiven">An array that tells which left edges are forced into the puzzle</param>
    /// <param name="upEdgeGiven">An array that tells which up edges are forced into the puzzle</param>
    /// <param name="ix">the index of the cell that is currently being generated</param>
    /// <returns></returns>
    private static IEnumerable<PuzzleState> GeneratePuzzleIndex(PuzzleState current, bool[] cellStateGiven, bool[] leftEdgeGiven, bool[] upEdgeGiven, int ix)
    {
        int size = current.Size;
        Cell[] cells = current.Cells;

        var x = ix % size;
        var y = ix / size;

        if (ix == size * size)
        {
            yield return current;
            yield break;
        }

        var valid = new List<CellState> { CellState.Zero, CellState.One };

        // Check that we don’t get more than two of the same digit in a straight row/column
        if (x >= 2 && cells[ix - 2].state == cells[ix - 1].state)
            valid.Remove(cells[ix - 1].state);
        if (y >= 2 && cells[ix - size].state == cells[ix - 2 * size].state)
            valid.Remove(cells[ix - size].state);

        // Check if the current row or column already contains enough 0’s or 1’s
        var zeros = Enumerable.Range(0, x).Count(c => cells[c + size * y].state == CellState.Zero);
        if (zeros >= size / 2)
            valid.Remove(CellState.Zero);
        else if (x - zeros >= size / 2)
            valid.Remove(CellState.One);
        zeros = Enumerable.Range(0, y).Count(r => cells[x + size * r].state == CellState.Zero);
        if (zeros == size / 2)
            valid.Remove(CellState.Zero);
        if (y - zeros == size / 2)
            valid.Remove(CellState.One);

        //Check if there is a contraint edge and verify it is satisfied
        valid.RemoveAll(v =>
        {
            if (cells[ix].Left != null &&
                !SatisfiesConstraint(v, cells[ix - 1].state, cells[ix].Left.Constraint))
                return true;

            if (cells[ix].Up != null &&
                !SatisfiesConstraint(v, cells[ix - size].state, cells[ix].Up.Constraint))
                return true;

            return false;
        });

        if (DistinctRows)
        {
            // Make sure that the row we just filled isn’t identical to an earlier row. We can check this one column early because the last digit is determined by the rest
            if (x == size - 2)
                for (var r = 0; r < y; r++)
                    if (Enumerable.Range(0, size - 2).All(c => cells[c + size * r].state == cells[c + size * y].state))
                        valid.Remove(cells[size - 2 + size * r].state);

            // Make sure that the column we just filled isn’t identical to an earlier column. We can check this one row early because the last digit is determined by the rest
            if (y == size - 2)
                for (var c = 0; c < x; c++)
                    if (Enumerable.Range(0, size - 2).All(r => cells[c + size * r].state == cells[x + size * r].state))
                        valid.Remove(cells[c + size * (size - 2)].state);
        }

        //Randomize results if there are multiple valid options to ensure a different puzzles each time
        valid = valid.Shuffle();

        //Choose a value for the current cell (and edges) and recursively generate the rest of the puzzle
        if (LongGeneration)
        {
            foreach (CellState v in valid)
            {
                //See what possible edge constraints there are for the current cell
                List<EdgeState> possibleLeftEdgeConstraints = new List<EdgeState>();
                List<EdgeState> possibleUpEdgeConstraints = new List<EdgeState>();

                if (cells[ix].Left != null)
                {
                    possibleLeftEdgeConstraints.Add(EdgeState.None);

                    if (SatisfiesConstraint(v, cells[ix - 1].state, EdgeState.Equal))
                        possibleLeftEdgeConstraints.Add(EdgeState.Equal);

                    else
                        possibleLeftEdgeConstraints.Add(EdgeState.X);
                }

                if (cells[ix].Up != null)
                {
                    possibleUpEdgeConstraints.Add(EdgeState.None);

                    if (SatisfiesConstraint(v, cells[ix - size].state, EdgeState.Equal))
                        possibleUpEdgeConstraints.Add(EdgeState.Equal);

                    else
                        possibleUpEdgeConstraints.Add(EdgeState.X);
                }

                //Randomize edge results if there are multiple valid options to ensure a different puzzles each time
                possibleLeftEdgeConstraints = possibleLeftEdgeConstraints.Shuffle();
                possibleUpEdgeConstraints = possibleUpEdgeConstraints.Shuffle();

                if (!cellStateGiven[ix] || v == cells[ix].state)
                {
                    cells[ix].state = v;

                    IEnumerable<EdgeState?> leftOptions = cells[ix].Left == null ? 
                                                          new[] { (EdgeState?)null } : possibleLeftEdgeConstraints.Select(e => (EdgeState?)e);

                    IEnumerable<EdgeState?> upOptions = cells[ix].Up == null ? 
                                                        new[] { (EdgeState?)null } : possibleUpEdgeConstraints.Select(e => (EdgeState?)e);

                    foreach (var left in leftOptions)
                    {
                        if (left.HasValue)
                            cells[ix].Left.Constraint = left.Value;

                        foreach (var up in upOptions)
                        {
                            if (up.HasValue)
                                cells[ix].Up.Constraint = up.Value;

                            foreach (var result in GeneratePuzzleIndex(current, cellStateGiven, leftEdgeGiven, upEdgeGiven, ix + 1))
                                yield return result;
                        }
                    }
                }
            }
        }

        else
        {
            foreach (CellState v in valid)
            {
                if (!cellStateGiven[ix] || v == cells[ix].state)
                {
                    cells[ix].state = v;
                    foreach (var result in GeneratePuzzleIndex(current, cellStateGiven, leftEdgeGiven, upEdgeGiven, ix + 1))
                        yield return result;
                }
            }
        }
    }

    /// <summary>
    /// Helper function that checks if a candidate cell state satisfies the constraint with its neighbor cell
    /// </summary>
    /// <param name="candidateState">The cell that is being checked</param>
    /// <param name="neighbor">The neighbor cell that is being checked</param>
    /// <param name="constraint">The edge case</param>
    /// <returns></returns>
    private static bool SatisfiesConstraint(CellState candidateState, CellState neighborState, EdgeState constraint)
    {
        //Verify that both cells are not empty, if at least one of them is empty then the constraint is satisfied
        if (candidateState == CellState.Empty || neighborState == CellState.Empty)
            return true;

        switch (constraint)
        {
            case EdgeState.None:
                return true;
            case EdgeState.Equal:
                return candidateState == neighborState;
            case EdgeState.X:
                return candidateState != neighborState;
            default:
                return false;

        }
    }

    /// <summary>
    /// Adds constraints to the genreated solution. Removes a percentage of the constraints from the generated puzzle based on the constraintRemovalPercentage variable to vary the difficulty of the puzzle. This is done after the solution is generated to reduce puzzle generation time.
    /// </summary>
    /// <param name="state"></param>
    private static void AddConstraints(PuzzleState state)
    {
        foreach (Edge nonEdge in state.Edges)
        {
            if (Rnd.Range(0, 100) < ConstraintRemovalPercentage)
                continue;

            if (nonEdge.CellA.state == nonEdge.CellB.state)
            {
                nonEdge.Constraint = EdgeState.Equal;
            }

            else
            {
                nonEdge.Constraint = EdgeState.X;
            }
        }
    }
}
