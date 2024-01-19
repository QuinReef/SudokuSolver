using System.Diagnostics;

namespace SudokuSolver.SudokuSolvers; 

public abstract class SudokuSolver {
    /// <summary>
    /// The current state of the <see cref="Sudoku"/> grid.
    /// </summary>
    private protected Sudoku ActiveSudoku;

    /// <summary>
    /// Timer to measure total time complexity.
    /// </summary>
    private protected Stopwatch Timer = new();

    protected SudokuSolver(Sudoku sudoku) {
        ActiveSudoku = sudoku;
        Timer.Start();
    }

    /// <summary>
    /// Performs the respective solving algorithm in order to solve the active <see cref="Sudoku"/>.
    /// </summary>
    public abstract void Solve();
}
