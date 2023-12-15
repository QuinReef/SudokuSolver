using System.Diagnostics;

namespace SudokuSolver;

public class SudokuSolver {
    // The current state of the sudoku grid.
    private Sudoku _activeSudoku;

    // Algorithm configurations.
    private bool _showSteps;
    private ushort _randomWalks;
    private ushort _localMaxLimit = 40;

    // Statistics to print to the console.
    private ushort _bestScore;
    private int _iterations;
    private ushort _walksEntered;

    // Timer to measure total time complexity.
    private Stopwatch _timer = new();
    // Instance for random values with an optional seed for consistent results.
    private const int seed = 53;
    private readonly Random _random = new(seed);

    public SudokuSolver(Sudoku s, ushort walks, bool showSteps) {
        _activeSudoku = s;

        _showSteps = showSteps;
        _randomWalks = walks;
        _bestScore = InitHeuristics(s);

        _timer.Start();
    }

    /// <summary>
    /// Evaluates the heuristic value for a row or column.
    /// </summary>
    private ushort Evaluate(ushort[] values) {
        HashSet<ushort> uniques = new();
        ushort counter = 0;

        foreach (ushort val in values) {
            /* If a value cannot be added to the hash set, it is a duplicate,
               which means that the heuristic score should be incremented. */
            if (!uniques.Add(val)) {
                counter++;
            }
        }

        return counter;
    }

    /// <summary>
    /// Initialises the initial heuristic values.
    /// </summary>
    public ushort InitHeuristics(Sudoku sudoku) {
        ushort score = 0;

        for (ushort i = 0; i < 9; i++) {
            // Separate computation to reduce complexity.
            ushort column = Evaluate(sudoku.GetColumnValues(i));
            ushort row = Evaluate(sudoku.GetRowValues(i));

            sudoku.GetHeuristicValues()[i] = column;
            sudoku.GetHeuristicValues()[9 + i] = row;

            score += (ushort)(column + row);
        }

        return score;
    }

    /// <summary>
    /// Updates the heuristic values of a sudoku puzzle after two cells were swapped.
    /// </summary>
    public ushort UpdateHeuristics(Sudoku sudoku, (ushort column, ushort row) coord1, (ushort column, ushort row) coord2) {
        // First value
        sudoku.GetHeuristicValues()[coord1.column] = Evaluate(sudoku.GetColumnValues(coord1.column));
        sudoku.GetHeuristicValues()[coord1.row + 9] = Evaluate(sudoku.GetRowValues(coord1.row));
        // Second value
        sudoku.GetHeuristicValues()[coord2.column] = Evaluate(sudoku.GetColumnValues(coord2.column));
        sudoku.GetHeuristicValues()[coord2.row + 9] = Evaluate(sudoku.GetRowValues(coord2.row));
        
        // Calculate the update heuristic score of all rows and columns.
        return ComputeSum(sudoku.GetHeuristicValues());
    }

    // Computes the sum of the given array.
    private ushort ComputeSum(ushort[] values) {
        ushort total = 0;

        for (ushort i = 0; i < values.Length; i++) {
            total += values[i];
        }

        return total;
    }

    /// <summary>
    /// Solves the sudoku using the Random Restart Hill Climbing algorithm
    /// </summary>
    public long Start() {
        int consecutiveIterationsWithoutImprovement = 0;

        ushort tempBestScore = _bestScore;
        ushort localMax = _bestScore;
        Sudoku currentBestSolution = _activeSudoku;

        while (_bestScore > 0) {
            (Sudoku, ushort) successor = DetermineBestSuccessor();

            // If the found successor is an improvement of the active sudoku..
            if (successor.Item2 < tempBestScore) {
                _activeSudoku = successor.Item1;
                tempBestScore = successor.Item2;

                // Adjust the best score and solution if the found local score is an improvement.
                if (tempBestScore <= _bestScore) {
                    _bestScore = tempBestScore;
                    currentBestSolution = _activeSudoku;
                }
            }

            // Else, the hill-climbing algorithm is stuck on a local maximum or plateau..
            else {
                consecutiveIterationsWithoutImprovement++;

                // Enter a random walk if the limit has been reached.
                if (consecutiveIterationsWithoutImprovement >= _localMaxLimit) {
                    // Return to the previous local maximum if the random walk has a higher local maximum.
                    if (tempBestScore > _bestScore) {
                        _activeSudoku = currentBestSolution;
                    }

                    // Perform a random walk.
                    RandomWalk();
                    tempBestScore = InitHeuristics(_activeSudoku);
                    localMax = tempBestScore;

                    consecutiveIterationsWithoutImprovement = 0;
                    _walksEntered++;
                }
            }

            // Print the relevant statistics to the console.
            if (_showSteps) { 
                PrintHillClimbStats(tempBestScore, localMax);
            } else {
                ShowElapsedTime();
            }

            _iterations++;
        }

        ShowFinalResult(currentBestSolution);
        // Return the total elapsed time for the experiments.
        return _timer.ElapsedMilliseconds;
    }

    /// <summary>
    /// Generates the succesors ordered by ascending heuristic value
    /// </summary>
    public (Sudoku, ushort) DetermineBestSuccessor() {
        (Sudoku sudoku, ushort currentBestFitness) bestSuccessor = new(_activeSudoku, ushort.MaxValue);

        // Randomly select a cluster
        ushort clusterIndex = (ushort)new Random().Next(0, 9);

        (ushort, ushort) ce1 = new(), ce2 = new();

        HashSet<(ushort, ushort)> nonFixedPositions = _activeSudoku.GetSudokuGrid()[clusterIndex].RetrieveInvalidCells();

        for (int i = 0; i < nonFixedPositions.Count; i++) {
            for (int j = i + 1; j < nonFixedPositions.Count; j++) {
                Sudoku clone = (Sudoku)_activeSudoku.Clone();
                SudokuCluster cluster = clone.GetSudokuGrid()[clusterIndex];

                (ushort, ushort) cell1 = nonFixedPositions.ElementAt(i);
                (ushort, ushort) cell2 = nonFixedPositions.ElementAt(j);

                cluster.SwapCells(cell1, cell2);

                (ushort, ushort) c1 = ((ushort)(cell1.Item1 + clusterIndex % 3 * 3), (ushort)(cell1.Item2 + (clusterIndex / 3) * 3));
                (ushort, ushort) c2 = ((ushort)(cell2.Item1 + clusterIndex % 3 * 3), (ushort)(cell2.Item2 + (clusterIndex / 3) * 3));

                ushort tempScore = UpdateHeuristics(clone, c1, c2);

                if (tempScore < bestSuccessor.currentBestFitness) {
                    ce1 = c1; ce2 = c2;
                    bestSuccessor = (clone, tempScore);
                }
            }
        }

        UpdateHeuristics(bestSuccessor.sudoku, ce1, ce2);
        return bestSuccessor;
    }

    /// <summary>
    /// Performs a random walk if the algorithm is stuck on a local maximum or plateau.
    /// </summary>
    public void RandomWalk() {
        for (int i = 0; i < _randomWalks; i++) {
            //// Randomly select a cluster
            ushort clusterIndex = (ushort)_random.Next(0, 9);

            SudokuCluster cluster = _activeSudoku.GetSudokuGrid()[clusterIndex];
            HashSet<(ushort, ushort)> nonFixedPositions = cluster.RetrieveInvalidCells();

            (ushort, ushort) cell1 = nonFixedPositions.ElementAt(_random.Next(0, nonFixedPositions.Count));
            (ushort, ushort) cell2;
            do cell2 = nonFixedPositions.ElementAt(_random.Next(0, nonFixedPositions.Count));
            while (cell1 == cell2);

            cluster.SwapCells(cell1, cell2);
        }
    }

    /// <summary>
    /// Prints the statistics for the hillclimb algorithm during runtime
    /// </summary>
    public void PrintHillClimbStats(ushort eval, ushort localMax) {
        // Temporarily stop the timer to capture the true processing time.
        _timer.Stop();

        if (_timer.ElapsedMilliseconds % 500 == 0) {
            Console.Clear();
            Console.WriteLine("┌───────────────────────────────┐");
            Console.WriteLine($"│ Timer: {_timer.Elapsed}\t│");
            Console.WriteLine($"│ Eval: {eval}, From: {localMax}\t\t│");
            Console.WriteLine($"│               Best: {_bestScore}\t\t│");
            Console.WriteLine($"│ Iterations: {_iterations}\t\t│");
            Console.WriteLine($"│ Random walks: {_walksEntered}\t\t│");
            Console.WriteLine($"│ Total walk steps: {_walksEntered * _randomWalks}\t│");
            Console.WriteLine("└───────────────────────────────┘");
            _activeSudoku.Show();
        }

        _timer.Start();
    }

    /// <summary>
    /// Print the elapsed time to the console
    /// </summary>
    public void ShowElapsedTime() {
        if (_timer.ElapsedMilliseconds % 10 == 0) {
            _timer.Stop();

            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"Current Time: {_timer.Elapsed}");
            _timer.Start();

        }
    }

    /// <summary>
    /// Prints the final result to the console
    /// </summary>
    /// <param name="finalSolution"></param>
    public void ShowFinalResult(Sudoku finalSolution) {
        Console.WriteLine($"Total Time: {_timer.Elapsed}                             ");
        Console.WriteLine($"Best found Score: {_bestScore}                           ");
        finalSolution.Show();
    }
}
