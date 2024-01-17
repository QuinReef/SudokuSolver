using System;
using System.Diagnostics;

namespace SudokuSolver
{

    public abstract class SudokuSolverBase
    {
        protected Sudoku _activeSudoku;
        protected Stopwatch _timer = new();

        protected SudokuSolverBase(Sudoku sudoku)
        {
            _activeSudoku = sudoku;
            _timer.Start();
        }

        public abstract bool Solve();

        protected bool SolveHelper()
        {
            var emptyCell = FindEmptyCell(_activeSudoku);

            // If there are no empty cells, the puzzle is solved.
            if (emptyCell == null)
            {
                _timer.Stop();
                Console.WriteLine(_timer.Elapsed);
                _activeSudoku.Show();
                return true;
            }

            var (row, column) = emptyCell.Item2;

            for (ushort value = 1; value <= 9; value++)
            {
                //If current assignment does not conflict with any constraints
                if (IsValidMove(_activeSudoku, row, column, value))
                {
                    //Try to assign the value to the cell.
                    AssignValue(row, column, value);

                    //Recursively try to solve the rest of the puzzle.
                    if (SolveHelper())
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

        protected abstract void AssignValue(ushort row, ushort column, ushort value);

        protected abstract void UndoMove(ushort row, ushort column);

        protected abstract Tuple<Cell, (ushort, ushort)>? FindEmptyCell(Sudoku sudoku);

        protected static bool IsValidMove(Sudoku sudoku, ushort row, ushort column, ushort value)
        {
            return !IsValueInRow(sudoku, row, value) &&
                   !IsValueInColumn(sudoku, column, value) &&
                   !IsValueInCluster(sudoku, row, column, value);
        }

        protected static bool IsValueInRow(Sudoku sudoku, ushort row, ushort value)
        {
            ushort[] rowValues = sudoku.GetRowValues(row);
            return rowValues.Contains(value);
        }

        protected static bool IsValueInColumn(Sudoku sudoku, ushort column, ushort value)
        {
            ushort[] columnValues = sudoku.GetColumnValues(column);
            return columnValues.Contains(value);
        }

        protected static bool IsValueInCluster(Sudoku sudoku, ushort row, ushort column, ushort value)
        {
            SudokuCluster cluster = sudoku.GetSudokuGrid()[row / 3 * 3 + column / 3];
            Cell[,] clusterCells = cluster.RetrieveCells();

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

