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
    protected abstract Tuple<Cell, (ushort, ushort)>? FindEmptyCell(Sudoku sudoku);

    protected abstract void AssignValue(ushort row, ushort column, ushort value);

    protected abstract void UndoMove(ushort row, ushort column);

    private protected bool SolveSudoku() {
        Tuple<Cell, (ushort, ushort)> emptyCell = FindEmptyCell(ActiveSudoku)!;

        // If there are no empty cells, the puzzle is solved.
        if (emptyCell == null) {
            Timer.Stop();
            Console.WriteLine(Timer.Elapsed);
            ActiveSudoku.Show();
            return true;
        }

        var (row, column) = emptyCell.Item2;

        for (ushort value = 1; value <= 9; value++)
        {
            //If current assignment does not conflict with any constraints
            if (IsValidMove(ActiveSudoku, row, column, value))
            {
                //Try to assign the value to the cell.
                AssignValue(row, column, value);

                //Recursively try to solve the rest of the puzzle.
                if (SolveSudoku())
                {
                    return true;
                }

                //If last move had no availible digits, undo the previous move
                UndoMove(row, column);
            }
        }
        // No valid value was found for the current empty cell.
        return false;
    }

    // Determine if a move abides by all sudoku rules.
    private bool IsValidMove(Sudoku sudoku, ushort row, ushort column, ushort value) {
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
