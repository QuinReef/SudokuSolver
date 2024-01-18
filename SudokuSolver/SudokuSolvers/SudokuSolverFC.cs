﻿using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace SudokuSolver.SudokuSolvers
{
    public class SudokuSolverFC
    {
        private Sudoku _activeSudoku;
        // Timer to measure total time complexity.
        private Stopwatch _timer = new();

        public SudokuSolverFC(Sudoku sudoku)
        {
            _activeSudoku = sudoku;
            _timer.Start();

        }

        public void Solve()
        {
            // Make the initial node consistent by updating domains based on fixed values.
            MakeNodeConsistent();

            // Start solving using forward checking.
            SolveHelper();

        }

        private bool SolveHelper()
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

            // Try assigning values from the domain to the empty cell.
            foreach (ushort value in new HashSet<ushort>(emptyCell.Item1.Domain))
            {
                if (IsValidMove(_activeSudoku, row, column, value))
                {
                    // Assign the value to the cell.
                    SudokuCluster cluster = _activeSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3];
                    cluster.AddCell(((ushort)(column % 3), (ushort)(row % 3)), new Cell(value, emptyCell.Item1.IsFixed, emptyCell.Item1.Domain));

                    // Forward checking: Update domains of affected cells.
                    UpdateDomains(row, column, cluster, value);
                    // Recursively try to solve the rest of the puzzle.
                    if (SolveHelper())
                    {
                        return true;
                    }

                    // If the recursive call fails, undo the move (backtrack) and restore domains.
                    cluster.AddCell(((ushort)(column % 3), (ushort)(row % 3)), emptyCell.Item1);
                    RestoreDomains(row, column, cluster, value);

                }
            }

            // No valid value was found for the current empty cell.
            return false;
        }

        protected Tuple<Cell, (ushort, ushort)>? FindEmptyCell(Sudoku sudoku) {
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

        private static bool IsValidMove(Sudoku sudoku, ushort row, ushort column, ushort value)
        {
            // Check if the value is not present in the row, column, and cluster.
            return !IsValueInRow(sudoku, row, value) &&
                   !IsValueInColumn(sudoku, column, value) &&
                   !IsValueInCluster(sudoku, row, column, value);
        }

        private static bool IsValueInRow(Sudoku sudoku, ushort row, ushort value)
        {
            HashSet<ushort> rowValues = sudoku.GetRowValues(row);
            return rowValues.Contains(value);
        }

        private static bool IsValueInColumn(Sudoku sudoku, ushort column, ushort value)
        {
            HashSet<ushort> columnValues = sudoku.GetColumnValues(column);
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

        public bool MakeNodeConsistent() {
            for (ushort row = 0; row < 9; row++) {
                for (ushort column = 0; column < 9; column++) {
                    SudokuCluster cluster = _activeSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3];
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
                    var cell = _activeSudoku.GetSudokuGrid()[row / 3 * 3 + c / 3].RetrieveCells()[c % 3, row % 3];
                    cell.Domain?.Remove(fixedValue);
                }
            }

            for (ushort r = 0; r < 9; r++)
            {
                if (r != row)
                {
                    var cell = _activeSudoku.GetSudokuGrid()[r / 3 * 3 + column / 3].RetrieveCells()[column % 3, r % 3];
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
                    _activeSudoku.GetSudokuGrid()[row / 3 * 3 + c / 3].RetrieveCells()[c % 3, row % 3].Domain?.Add(fixedValue);
                }
            }

            for (ushort r = 0; r < 9; r++)
            {
                if (r != row)
                {
                    _activeSudoku.GetSudokuGrid()[r / 3 * 3 + column / 3].RetrieveCells()[column % 3, r % 3].Domain?.Add(fixedValue);
                }
            }
            cluster.RestoreDomains(fixedValue);

        }

    }
}
