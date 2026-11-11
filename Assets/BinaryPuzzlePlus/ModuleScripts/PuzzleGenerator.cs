using System.Collections.Generic;
using System.Linq;
public static class PuzzleGenerator {

    //the width and length of the grid
    public static int Size;

    //If rows/columns should be distinct from one another
    public static bool DistinctRows;

    /// <summary>
    /// Generate the puzzle solution.
    /// </summary>
    /// <returns>The generated puzzle</returns>
    public static PuzzleState GenerateSolution()
    {
        //Generate the cells first, then edges afterwards.
        PuzzleState solution = new PuzzleState(Size);

        solution = GeneratePuzzleIndex(solution, new bool[Size * Size], new bool[Size * Size], new bool[Size * Size], 0).First();
        AddConstraints(solution);
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


        //Check if the current cell satisfies the constraints with its neighbors, only if the left/up edge is given, otherwise the constraint is satisfied
        valid.RemoveAll(v =>
        {
            //if the left edge is given, only use that edge state
            if (cells[ix].Left != null && leftEdgeGiven[ix] &&
                !SatisfiesConstraint(v, cells[ix - 1].state, cells[ix].Left.Constraint))
                return true;

            //if the up edge is given, only use that edge state
            if (cells[ix].Up != null && upEdgeGiven[ix] &&
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
        valid = GeneralExtensions.Shuffle(valid);

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
                throw new System.ArgumentOutOfRangeException(nameof(constraint), constraint, null);
        }
    }

   
    /// <summary>
    /// Adds constraints to the genreated solution.
    /// </summary>
    /// <param name="state"></param>
    private static void AddConstraints(PuzzleState state)
    {
        foreach (Edge nonEdge in state.Edges)
        {

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

    /// <summary>
    /// Reduces the constraints in the puzzle solution.
    /// </summary>
    /// <param name="solution">The puzzle state whose constraints are reduced.</param>
    /// <returns>The </returns>
    public static PuzzleState ReduceConstraints(PuzzleState solution)
    {
        
        //Create a list of the constraints to figure out what should be given
        int puzzleSize = Size * Size;

        //The the amount of constraints to give, one for each cell, one for each left edge, and one for each up edge
        int constraintSize = puzzleSize * 3;

        
        //figure out which what constraints to give
        int[] puzzleIxs = Ut.ReduceRequiredSet(Ut.Shuffle(Enumerable.Range(0, constraintSize).ToArray()), test =>
        {
            //Make a copy of the solution to avoid modifying the original puzzle state
            PuzzleState solutionClone = new PuzzleState(solution);

            //First size * size indicies are the cells, then the left edges, then the up edges
            bool[] givenConstraints = new bool[constraintSize];

            foreach (var ix in test.SetToTest)
                givenConstraints[ix] = true;

            //convert the given constraints into a puzzle state
            bool[] givenCells = givenConstraints.Take(puzzleSize).ToArray();
            bool[] givenLeftEdges = givenConstraints.Skip(puzzleSize).Take(puzzleSize).ToArray();
            bool[] givenUpEdges = givenConstraints.Skip(puzzleSize * 2).Take(puzzleSize).ToArray();
            return GeneratePuzzleIndex(solutionClone, givenCells, givenLeftEdges, givenUpEdges, 0).Take(2).Count() == 1;
        }).ToArray();


        //Make a copy of the puzzle, and remove the constraints that are not given
        PuzzleState given = new PuzzleState(solution);

        for(int i = 0; i < puzzleSize; i++)
        {
            int row = i / Size;
            int col = i % Size;
            //remove the cell
            if (!puzzleIxs.Contains(i))
            {
                given.Cells[i].state = CellState.Empty;
            }
            //remove the left edge
            if (col > 0 && !puzzleIxs.Contains(puzzleSize + i))
            {
                given.Cells[i].Left.Constraint = EdgeState.None;
            }
            //remove the up edge
            if (row > 0 && !puzzleIxs.Contains((puzzleSize * 2) + i))
            {
                given.Cells[i].Up.Constraint = EdgeState.None;
            }
        }

        return given;
    }
}
