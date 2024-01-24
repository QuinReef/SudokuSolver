namespace SudokuSolver.SudokuSolvers;

public class SudokuSolverFCMCV : SudokuSolverFC {
    public SudokuSolverFCMCV(Sudoku sudoku, bool showSteps, int interval) : base(sudoku, showSteps, interval) { }

    private protected override Tuple<Cell, (ushort, ushort)>? FindEmptyCell() {
        Tuple<Cell, (ushort, ushort)> smallestEmptyCell = null!;

        for (ushort row = 0; row < 9; row++) {
            for (ushort column = 0; column < 9; column++) {
                Cell cell = ActiveSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3].RetrieveCells()[column % 3, row % 3];

                if (cell.Value == 0) {
                    int domainSize = cell.Domain?.Count ?? 0;

                    /* Only adjust the currently selected empty cell if it is either null,
                       or its domain is bigger than or equal to the newly found cell's domain. */
                    if (smallestEmptyCell == null || domainSize < smallestEmptyCell.Item1.Domain?.Count) {
                        smallestEmptyCell = new Tuple<Cell, (ushort, ushort)>(cell, (row, column));
                    }
                }
            }
        }

        // Either returns the first empty cell, or null if no empty cell was found.
        return smallestEmptyCell;
    }
}
