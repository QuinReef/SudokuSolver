using System.Diagnostics;

namespace SudokuSolver.SudokuSolvers; 

/// <summary>
/// Represents an algorithm to solve an arbitrary <see cref="Sudoku"/> puzzle.
/// </summary>
public abstract class SudokuSolver {
    /// <summary>
    /// The current state of the <see cref="Sudoku"/> grid.
    /// </summary>
    private protected Sudoku ActiveSudoku;

    /// <summary>
    /// Indicates whether or not to display intermediate steps on the console.
    /// </summary>
    private protected bool ShowSteps;

    /// <summary>
    /// Timer to measure total time complexity.
    /// </summary>
    private protected Stopwatch Timer = new();

    protected SudokuSolver(Sudoku sudoku, bool showSteps) {
        ActiveSudoku = sudoku;
        ShowSteps = showSteps;
        Timer.Start();
    }

    /// <summary>
    /// Performs the respective solving algorithm.
    /// </summary>
    public abstract void Solve();

    /// <summary>
    /// Prints relevant statistics to the console.
    /// </summary>
    private protected abstract void PrintStats();

    /// <summary>
    /// Prints the final, solved state to the console with some relevant statistics.
    /// </summary>
    private protected abstract void ShowFinalResult(Sudoku solution);

    /// <summary>
    /// Prints the appropriate information to the console.
    /// </summary>
    private protected void Print() {
        if (ShowSteps) {
            PrintStats();
        } else {
            ShowElapsedTime();
        }
    }

    // Prints the active run time of the algorithm to the console.
    private void ShowElapsedTime() {
        if (Timer.ElapsedMilliseconds % 10 == 0) {
            Timer.Stop();

            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"Current Time: {Timer.Elapsed}");

            Timer.Start();
        }
    }
}
