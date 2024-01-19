namespace SudokuSolver.SudokuSolvers; 

public class SudokuSolverCBT : SudokuSolver {
    public SudokuSolverCBT(Sudoku sudoku) : base(sudoku) { }

    public override void Solve() => SolveSudoku();

    private bool SolveSudoku() {
        Tuple<Cell, (ushort, ushort)> emptyCell = FindEmptyCell(ActiveSudoku)!;

        // If no empty cells remain, the puzzle is solved.
        if (CheckIfDone(emptyCell)) {
            return true;
        }

        // Obtain the coordinates of the empty cell in the Sudoku.
        (ushort row, ushort column) = emptyCell.Item2;

        for (ushort value = 1; value <= 9; value++) {
            //If current assignment does not conflict with any constraints
            if (IsValidMove(ActiveSudoku, row, column, value)) {
                SudokuCluster cluster = ActiveSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3];

                //Try to assign the value to the cell.
                PerformMove((column, row), cluster, new Cell(value, emptyCell.Item1.IsFixed));

                //Recursively try to solve the rest of the puzzle.
                if (SolveSudoku()) {
                    return true;
                }

                // If the last move had no availible digits, undo the previous move.
                PerformMove((column, row), cluster, new Cell(0, false));
            }
        }

        // No valid value was found for the current empty cell.
        return false;
    }
}
