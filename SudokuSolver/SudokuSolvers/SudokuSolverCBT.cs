namespace SudokuSolver.SudokuSolvers; 

public class SudokuSolverCBT : SudokuSolver {
    public SudokuSolverCBT(Sudoku sudoku, bool showSteps, int interval) : base(sudoku, showSteps, interval) { }

    public override void Solve() => SolveRecursion();
    
    private protected bool SolveRecursion() {
        // Leave recursion if no more empty cells were found.
        if (CheckIfDone(out Tuple<Cell, (ushort, ushort)>? emptyCell)) {
            return true;
        }

        // Print relevant information to the console.
        Print();

        return SolveSudoku(emptyCell);
    }

    private protected virtual bool SolveSudoku(Tuple<Cell, (ushort, ushort)>? emptyCell) {
        // Obtain the coordinates of the first empty cell in the sudoku.
        (ushort row, ushort column) = emptyCell!.Item2;

        for (ushort value = 1; value <= 9; value++) {
            if (IsValidMove(ActiveSudoku, row, column, value)) {
                SudokuCluster cluster = ActiveSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3];

                // Assign the value to the empty cell.
                PerformMove((column, row), cluster, new Cell(value, emptyCell.Item1.IsFixed));
                Steps++;

                // Recursively try to solve the rest of the puzzle.
                if (SolveRecursion()) {
                    return true;
                }

                // If the previous move had no availible digits, undo the move.
                PerformMove((column, row), cluster, new Cell(0, false));
                BackTracks++;
            }
        }

        // No valid value was found for the current empty cell.
        return false;
    }

    /// <summary>
    /// Attempts to find an empty <see cref="Cell"/>, and determines whether the algorithm has finished,
    /// optionally returning the first empty <see cref="Cell"/>.
    /// </summary>
    /// <returns><c>True</c> if the algorithm has finished, <c>False</c> otherwise.</returns>
    private protected bool CheckIfDone(out Tuple<Cell, (ushort, ushort)>? cell) {
        cell = FindEmptyCell()!;

        if (cell == null) {
            Timer.Stop();
            ShowFinalResult();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Adjusts the value of a <see cref="Cell"/> within a <see cref="SudokuCluster"/>.
    /// </summary>
    /// <param name="loc">The coordinates of the <see cref="Cell"/> in the <see cref="SudokuCluster"/>.</param>
    /// <param name="cluster">The respective cluster in the <see cref="Sudoku"/> grid.</param>
    /// <param name="cell">The <see cref="Cell"/> instance to replace at the specified coordinates.</param>
    private protected void PerformMove((ushort column, ushort row) loc, SudokuCluster cluster, Cell cell) {
        cluster.AddCell(((ushort)(loc.column % 3), (ushort)(loc.row % 3)), cell);
    }

    /// <summary>
    /// Finds the first empty <see cref="Cell"/> in a <see cref="Sudoku"/> grid.
    /// </summary>
    /// <returns>The first empty <see cref="Cell"/> found in a row-based search, <c>null</c> otherwise.</returns>
    private protected virtual Tuple<Cell, (ushort, ushort)>? FindEmptyCell() {
        for (ushort row = 0; row < 9; row++) {
            for (ushort column = 0; column < 9; column++) {
                Cell cell = ActiveSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3].RetrieveCells()[column % 3, row % 3];
                if (cell.Value == 0) {
                    return new Tuple<Cell, (ushort, ushort)>(cell, (row, column));
                }
            }
        }

        // If there are no empty cells, return null.
        return null;
    }

    // Determine if a move abides by all sudoku rules.
    private protected bool IsValidMove(Sudoku sudoku, ushort row, ushort column, ushort value) {
        return !IsValueInRow(sudoku, row, value)
               && !IsValueInColumn(sudoku, column, value)
               && !IsValueInCluster(sudoku, row, column, value);
    }

    private bool IsValueInRow(Sudoku sudoku, ushort row, ushort value) {
        HashSet<ushort> rowValues = sudoku.GetRowValues(row);
        return rowValues.Contains(value);
    }

    private bool IsValueInColumn(Sudoku sudoku, ushort column, ushort value) {
        HashSet<ushort> columnValues = sudoku.GetColumnValues(column);
        return columnValues.Contains(value);
    }

    private bool IsValueInCluster(Sudoku sudoku, ushort row, ushort column, ushort value) {
        SudokuCluster cluster = sudoku.GetSudokuGrid()[row / 3 * 3 + column / 3];
        Cell[,] clusterCells = cluster.RetrieveCells();

        for (ushort i = 0; i < 3; i++) {
            for (ushort j = 0; j < 3; j++) {
                if (clusterCells[i, j].Value == value) {
                    return true;
                }
            }
        }

        return false;
    }

    private protected override void PrintStats() {
        /* Temporarily stop the timer to solely include computation time,
           as printing takes a considerable amount of time. */
        Timer.Stop();

        if (Timer.ElapsedMilliseconds % PrintInterval == 0) {
            Console.WriteLine();
            Console.WriteLine("┌───────────────────────────────┐");
            Console.WriteLine($"│ Timer: {Timer.Elapsed}\t│");
            Console.WriteLine("└───────────────────────────────┘");
            Console.WriteLine();
            ActiveSudoku.Show();
        }

        Timer.Start();
    }

    private protected override void ShowFinalResult() {
        Console.WriteLine();

        // If no intermediate steps were shown, remove the "current timer" and previous empty line.
        if (!ShowSteps) {
            Console.Clear();
        }

        Console.WriteLine("┌───────────────────────────────┐");
        Console.WriteLine($"│ Total Time: {Timer.Elapsed}\t│");
        Console.WriteLine($"│ Total Steps: {Steps}\t\t│");
        Console.WriteLine($"│ Total Backtracks: {BackTracks}\t│");
        Console.WriteLine("└───────────────────────────────┘");

        ActiveSudoku.Show();
    }
}
