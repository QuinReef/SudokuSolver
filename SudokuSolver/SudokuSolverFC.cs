using System;
namespace SudokuSolver
{
    public class SudokuSolverFC
    {
        private Sudoku _activeSudoku;

        public SudokuSolverFC(Sudoku _sudoku)
        {
            _activeSudoku = _sudoku;
        }

        public bool Solve()
        {
            return SolveHelper();
        }

        private bool SolveHelper()
        {
            var emptyCell = FindEmptyCell(_activeSudoku);

            // If there are no empty cells, the puzzle is solved.
            if (emptyCell == null) {
                return true;
            }

            var (row, column) = emptyCell.Value;

            // Try assigning values from 1 to 9 to the empty cell.
            for (ushort value = 1; value <= 9; value++)
            {
                if (IsValidMove(_activeSudoku, row, column, value))
                {
                    // Assign the value to the cell.
                    _activeSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3].AddCell(((ushort)(column % 3), (ushort)(row % 3)), value);
                    _activeSudoku.Show();
                    // Recursively try to solve the rest of the puzzle.
                    if (SolveHelper())
                    {
                        return true;
                    }

                    // If the recursive call fails, undo the move (backtrack).
                    _activeSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3].AddCell(((ushort)(column % 3), (ushort)(row % 3)), 0);
                }
            }

            // No valid value was found for the current empty cell.
            return false;
        }

        private static (ushort, ushort)? FindEmptyCell(Sudoku sudoku)
        {
            for (ushort row = 0; row < 9; row++)
            {
                for (ushort column = 0; column < 9; column++)
                {
                    if (sudoku.GetSudokuGrid()[row / 3 * 3 + column / 3].RetrieveCells()[column % 3, row % 3].Value == 0)
                    {
                        return (row, column);
                    }
                }
            }

            // If there are no empty cells, return null.
            return null;
        }

        //Invariant 
        private static bool IsValidMove(Sudoku sudoku, ushort row, ushort column, ushort value)
        {
            // Check if the value is not present in the row, column, and cluster.
            return !IsValueInRow(sudoku, row, value) &&
                   !IsValueInColumn(sudoku, column, value) &&
                   !IsValueInCluster(sudoku, row, column, value);
        }

        private static bool IsValueInRow(Sudoku sudoku, ushort row, ushort value)
        {
            ushort[] rowValues = sudoku.GetRowValues(row);
            return rowValues.Contains(value);
        }

        private static bool IsValueInColumn(Sudoku sudoku, ushort column, ushort value)
        {
            ushort[] columnValues = sudoku.GetColumnValues(column);
            return columnValues.Contains(value);
        }

        private static bool IsValueInCluster(Sudoku sudoku, ushort row, ushort column, ushort value)
        {
            SudokuCluster cluster = sudoku.GetSudokuGrid()[row / 3 * 3 + column / 3];
            Cell[,] clusterCells = cluster.RetrieveCells();

            // Check if the value is present in the 3x3 cluster.
            for (ushort i = 0; i < 3; i++)
            {
                for (ushort j = 0; j < 3; j++)
                {
                    if (clusterCells[i, j].Value == value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}


