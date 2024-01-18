using System;
namespace SudokuSolver.SudokuSolvers; 

public class SudokuSolverCBT : SudokuSolver {
    public SudokuSolverCBT(Sudoku sudoku) : base(sudoku) { }

    public override void Solve() => SolveSudoku();

    protected override void AssignValue(ushort row, ushort column, ushort value)
    {
        ActiveSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3].AddCell(((ushort)(column % 3), (ushort)(row % 3)), value);
    }
         
    protected override void UndoMove(ushort row, ushort column) {
        ActiveSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3].AddCell(((ushort)(column % 3), (ushort)(row % 3)), 0);
    }

    // protected override Tuple<Cell, (ushort, ushort)>? FindEmptyCell(Sudoku sudoku)
    // {
    //     for (ushort row = 0; row < 9; row++)
    //     {
    //         for (ushort column = 0; column < 9; column++)
    //         {
    //             if (sudoku.GetSudokuGrid()[row / 3 * 3 + column / 3].RetrieveCells()[column % 3, row % 3].Value == 0)
    //             {
    //                 return Tuple.Create(
    //                     sudoku.GetSudokuGrid()[row / 3 * 3 + column / 3].RetrieveCells()[column % 3, row % 3],
    //                     (row, column)
    //                 );
    //             }
    //         }
    //     }
    //
    //     return null;
    // }

    protected override Tuple<Cell, (ushort, ushort)>? FindEmptyCell(Sudoku sudoku) {
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
}
