namespace SudokuSolver.SudokuSolvers;

public class SudokuSolverFCMCV : SudokuSolverFC {
    public SudokuSolverFCMCV(Sudoku sudoku, bool showSteps) : base(sudoku, showSteps) { }

    public override void Solve() {
        /* Firstly, make the initial Sudoku node consistent by updating domains based on fixed values,
           then start solving using Forward Checking. */
        MakeNodeConsistent();
        SolveSudoku();
    }

    protected override Tuple<Cell, (ushort, ushort)>? FindEmptyCeell() {
        int minDomainSize = int.MaxValue;
        (ushort cl, ushort r) = (0, 0);
        bool b = false;
        Cell c = new Cell(0, false);

        for (ushort row = 0; row < 9; row++) {
            for (ushort column = 0; column < 9; column++) {
                c = ActiveSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3].RetrieveCells()[column % 3, row % 3];
                if (c.Value == 0) { // empty
                    int domainSize = c.Domain?.Count ?? 0;

                    if (domainSize < minDomainSize) {
                        minDomainSize = domainSize;
                        cl = column; r = row;
                        b = true;
                    }
                }
            }
        }

        if (!b) {
            Console.WriteLine($"| {c} |");
        }

        Console.WriteLine($"{cl} - {r}");
        return !b ? null : new Tuple<Cell, (ushort, ushort)>(c, (cl, r));
    }

    private bool SolveSudoku()
    {
        // If no empty cells remain, the puzzle is solved.
        if (CheckIfDone(out Tuple<Cell, (ushort, ushort)>? emptyCell)) {
            return true;
        }

        // Print relevant information to the console.
        Print();

        // Obtain the coordinates of the first empty cell in the sudoku.
        (ushort row, ushort column) = FindEmptyCeell().Item2;

        // Try assigning values from the domain to the empty cell.
        foreach (ushort value in new HashSet<ushort>(emptyCell.Item1.Domain!))
        {
            if (IsValidMove(ActiveSudoku, row, column, value))
            {
                SudokuCluster cluster = ActiveSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3];

                // Assign the value to the empty cell.
                PerformMove((column, row), cluster, new Cell(value, emptyCell.Item1.IsFixed, emptyCell.Item1.Domain!));

                // Update the domains of the affected cells.
                UpdateDomains((column, row), cluster, value, false);

                // Recursively try to solve the rest of the puzzle.
                if (SolveSudoku())
                {
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
}
