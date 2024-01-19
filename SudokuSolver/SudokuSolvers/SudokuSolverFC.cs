namespace SudokuSolver.SudokuSolvers; 

public class SudokuSolverFC : SudokuSolver {
    public SudokuSolverFC(Sudoku sudoku) : base(sudoku) { }

    public override void Solve() {
        /* Firstly, make the initial Sudoku node consistent by updating domains based on fixed values,
           then start solving using Forward Checking. */
        MakeNodeConsistent();
        SolveSudoku();
    }

    private bool SolveSudoku() {
        Tuple<Cell, (ushort, ushort)> emptyCell = FindEmptyCell(ActiveSudoku)!;

        // If no empty cells remain, the puzzle is solved.
        if (CheckIfDone(emptyCell)) {
            return true;
        }

        // Obtain the coordinates of the empty cell in the Sudoku.
        (ushort row, ushort column) = emptyCell.Item2;

        // Try assigning values from the domain to the empty cell.
        foreach (ushort value in new HashSet<ushort>(emptyCell.Item1.Domain!)) {
            if (IsValidMove(ActiveSudoku, row, column, value)) {
                // Assign the value to the cell.
                SudokuCluster cluster = ActiveSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3];
                
                PerformMove((column, row), cluster, new Cell(value, emptyCell.Item1.IsFixed, emptyCell.Item1.Domain!));

                // Forward checking: Update domains of affected cells.
                UpdateDomains(row, column, cluster, value);

                // Recursively try to solve the rest of the puzzle.
                if (SolveSudoku())
                {
                    return true;
                }

                // If the recursive call fails, undo the move (backtrack) and restore domains.
                PerformMove((column, row), cluster, emptyCell.Item1);

                RestoreDomains(row, column, cluster, value);
            }
        }

        // No valid value was found for the current empty cell.
        return false;
    }

    public bool MakeNodeConsistent() {
        for (ushort row = 0; row < 9; row++) {
            for (ushort column = 0; column < 9; column++) {
                SudokuCluster cluster = ActiveSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3];
                Cell cell = cluster.RetrieveCells()[column % 3, row % 3];

                // Check if the cell has a fixed value.
                if (cell.IsFixed) {
                    // Remove the fixed value from the domains of cells in the same row, column, and cluster.
                    UpdateDomains(row, column, cluster, cell.Value);
                }
            }
        }

        return true;
    }

    private void UpdateDomains(ushort row, ushort column, SudokuCluster cluster, ushort fixedValue)
    {
        // Update domains in the same row, column, and cluster.

        for (ushort c = 0; c < 9; c++)
        {
            if (c != column)
            {
                var cell = ActiveSudoku.GetSudokuGrid()[row / 3 * 3 + c / 3].RetrieveCells()[c % 3, row % 3];
                cell.Domain?.Remove(fixedValue);
            }
        }

        for (ushort r = 0; r < 9; r++)
        {
            if (r != row)
            {
                var cell = ActiveSudoku.GetSudokuGrid()[r / 3 * 3 + column / 3].RetrieveCells()[column % 3, r % 3];
                cell.Domain?.Remove(fixedValue);
            }
        }
        cluster.UpdateDomains(fixedValue);

    }

    private void RestoreDomains(ushort row, ushort column, SudokuCluster cluster, ushort fixedValue)
    {

        for (ushort c = 0; c < 9; c++)
        {
            if (c != column)
            {
                ActiveSudoku.GetSudokuGrid()[row / 3 * 3 + c / 3].RetrieveCells()[c % 3, row % 3].Domain?.Add(fixedValue);
            }
        }

        for (ushort r = 0; r < 9; r++)
        {
            if (r != row)
            {
                ActiveSudoku.GetSudokuGrid()[r / 3 * 3 + column / 3].RetrieveCells()[column % 3, r % 3].Domain?.Add(fixedValue);
            }
        }
        cluster.RestoreDomains(fixedValue);

    }
}