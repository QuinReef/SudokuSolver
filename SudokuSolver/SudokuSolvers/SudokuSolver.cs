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
    /// Indicates how often, in milliseconds, the <see cref="Sudoku"/> grid should be printed to the console.
    /// </summary>
    private protected int PrintInterval;

    /// <summary>
    /// Timer to measure total time complexity.
    /// </summary>
    private protected Stopwatch Timer = new();

    protected SudokuSolver(Sudoku sudoku, bool showSteps, int interval) {
        ActiveSudoku = sudoku;
        ShowSteps = showSteps;
        PrintInterval = interval;
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
    private protected abstract void ShowFinalResult();

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
        Timer.Stop();

        if (Timer.ElapsedMilliseconds % 1 == 0) {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"Current Time: {Timer.Elapsed}");
        }

        Timer.Start();
    }
}
