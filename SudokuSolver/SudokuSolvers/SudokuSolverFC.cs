namespace SudokuSolver.SudokuSolvers; 

public class SudokuSolverFC : SudokuSolverCBT {
    public SudokuSolverFC(Sudoku sudoku, bool showSteps, int interval) : base(sudoku, showSteps, interval) { }

    public override void Solve() {
        /* Firstly, make the initial Sudoku node consistent by updating domains based on fixed values,
           then start solving using Forward Checking. */
        MakeNodeConsistent();
        SolveRecursion();
    }

    private protected override bool SolveSudoku(Tuple<Cell, (ushort, ushort)>? emptyCell) {
        // Obtain the coordinates of the first empty cell in the sudoku.
        (ushort row, ushort column) = emptyCell!.Item2;

        // Try assigning values from the domain to the empty cell.
        foreach (ushort value in new HashSet<ushort>(emptyCell.Item1.Domain!)) {
            if (IsValidMove(ActiveSudoku, row, column, value)) {
                SudokuCluster cluster = ActiveSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3];

                // Assign the value to the empty cell.
                PerformMove((column, row), cluster, new Cell(value, emptyCell.Item1.IsFixed, emptyCell.Item1.Domain!));

                // Update the domains of the affected cells.
                UpdateDomains((column, row), cluster, value, false);

                // Recursively try to solve the rest of the puzzle.
                if (SolveRecursion()) {
                    return true;
                }

                // If the previous move had no availible digits, undo the move and restore the domains.
                PerformMove((column, row), cluster, emptyCell.Item1);
                UpdateDomains((column, row), cluster, value, true);
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
                    UpdateDomains((column, row), cluster, cell.Value, false);
                }
            }
        }

        return true;
    }

    /* Updates the domains of an affected cell, specified by its location.
       If "restore" is set to store, it will instead restore the domains by removing the value. */
    private protected void UpdateDomains((ushort column, ushort row) loc, SudokuCluster cluster, ushort value, bool restore) {
        // Update domains in the same column.
        for (ushort c = 0; c < 9; c++) {
            if (c != loc.column) {
                Cell cell = ActiveSudoku.GetSudokuGrid()[loc.row / 3 * 3 + c / 3].RetrieveCells()[c % 3, loc.row % 3];

                if (restore) {
                    cell.Domain?.Add(value);
                } else {
                    cell.Domain?.Remove(value);
                }
            }
        }

        // Update domains in the same row.
        for (ushort r = 0; r < 9; r++) {
            if (r != loc.row) {
                Cell cell = ActiveSudoku.GetSudokuGrid()[r / 3 * 3 + loc.column / 3].RetrieveCells()[loc.column % 3, r % 3];

                if (restore) {
                    cell.Domain?.Add(value);
                } else {
                    cell.Domain?.Remove(value);
                }
            }
        }

        // Update domains in the same cluster.
        foreach ((ushort x, ushort y) in cluster.RetrieveInvalidCells()) {
            if (restore) {
                cluster.RetrieveCells()[x, y].Domain!.Add(value);
            } else {
                cluster.RetrieveCells()[x, y].Domain!.Remove(value);
            }
        }
    }
}
