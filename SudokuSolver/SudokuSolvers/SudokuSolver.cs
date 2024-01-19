using System.Diagnostics;

namespace SudokuSolver.SudokuSolvers; 

public abstract class SudokuSolver {
    // The current state of the sudoku grid.
    private protected Sudoku ActiveSudoku;

    // Timer to measure total time complexity.
    private protected Stopwatch Timer = new();

    protected SudokuSolver(Sudoku sudoku) {
        ActiveSudoku = sudoku;
        Timer.Start();
    }
    
    /// <summary>
    /// Performs the solving algorithm in order to solve the active <see cref="Sudoku"/>.
    /// </summary>
    public abstract void Solve();

    private protected void PerformMove((ushort column, ushort row) loc, SudokuCluster cluster, Cell cell) {
        cluster.AddCell(((ushort)(loc.column % 3), (ushort)(loc.row % 3)), cell);
    }

    private protected bool CheckIfDone(Tuple<Cell, (ushort, ushort)>? cell) {
        if (cell == null) {
            Timer.Stop();
            Console.WriteLine(Timer.Elapsed);
            ActiveSudoku.Show();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Finds the first empty cell in a Sudoku grid.
    /// </summary>
    /// <returns>The first empty <see cref="Cell"/> found in a row-based search, <c>null</c> otherwise.</returns>
    private protected Tuple<Cell, (ushort, ushort)>? FindEmptyCell(Sudoku sudoku) {
        for (ushort row = 0; row < 9; row++) {
            for (ushort column = 0; column < 9; column++) {
                Cell cell = sudoku.GetSudokuGrid()[row / 3 * 3 + column / 3].RetrieveCells()[column % 3, row % 3];
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
}
