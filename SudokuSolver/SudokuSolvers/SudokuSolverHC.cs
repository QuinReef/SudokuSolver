﻿namespace SudokuSolver.SudokuSolvers;

public class SudokuSolverHC : SudokuSolver {
    // Algorithm configurations.
    private ushort _randomWalks;
    private ushort _localMaxLimit = 40;
    private int _maxIterations = 1000000;

    // Statistics to print to the console.
    private ushort _bestScore;
    private int _iterations;
    private ushort _walksEntered;
    private ushort _localMax;
    private ushort _evalScore;

    // Instance for random values with an optional seed for consistent results.
    private const int seed = 53;
    private readonly Random _random = new(seed);

    public SudokuSolverHC(Sudoku sudoku, bool showSteps, int interval, ushort walks) : base(sudoku, showSteps, interval) {
        _randomWalks = walks;
        _bestScore = InitHeuristics(sudoku);
    }

    /// <summary>
    /// Evaluates the heuristic value for a row or column.
    /// </summary>
    private ushort Evaluate(HashSet<ushort> values) {
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
    private ushort InitHeuristics(Sudoku sudoku) {
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
    private ushort UpdateHeuristics(Sudoku sudoku, (ushort column, ushort row) coord1, (ushort column, ushort row) coord2) {
        // First value
        sudoku.GetHeuristicValues()[coord1.column] = Evaluate(sudoku.GetColumnValues(coord1.column));
        sudoku.GetHeuristicValues()[coord1.row + 9] = Evaluate(sudoku.GetRowValues(coord1.row));
        // Second value
        sudoku.GetHeuristicValues()[coord2.column] = Evaluate(sudoku.GetColumnValues(coord2.column));
        sudoku.GetHeuristicValues()[coord2.row + 9] = Evaluate(sudoku.GetRowValues(coord2.row));

        // Calculate the update heuristic score of all rows and columns.
        return ComputeSum(sudoku.GetHeuristicValues());
    }

    private ushort ComputeSum(ushort[] values) {
        ushort total = 0;

        for (ushort i = 0; i < values.Length; i++) {
            total += values[i];
        }

        return total;
    }

    /// <summary>
    /// Performs the hill-climbing algorithm with a random-walk implementation.
    /// </summary>
    public override void Solve() {
        int consecutiveIterationsWithoutImprovement = 0;

        _evalScore = _bestScore;
        _localMax = _bestScore;
        Sudoku currentBestSolution = (Sudoku)ActiveSudoku.Clone();

        while (_bestScore > 0 && _iterations < _maxIterations) {
            (Sudoku, ushort) successor = DetermineBestSuccessor();

            // If the found successor is an improvement of the active sudoku..
            if (successor.Item2 < _evalScore) {
                ActiveSudoku = (Sudoku)successor.Item1.Clone();
                _evalScore = successor.Item2;

                // Adjust the best score and solution if the found local score is an improvement.
                if (_evalScore <= _bestScore) {
                    _bestScore = _evalScore;
                    currentBestSolution = (Sudoku)ActiveSudoku.Clone();
                }
            }

            // Else, the hill-climbing algorithm is stuck on a local maximum or plateau..
            else {
                consecutiveIterationsWithoutImprovement++;

                // Enter a random walk if the limit has been reached.
                if (consecutiveIterationsWithoutImprovement >= _localMaxLimit) {
                    // Return to the previous local maximum if the random walk has a higher local maximum.
                    if (_evalScore > _bestScore) {
                        ActiveSudoku = (Sudoku)currentBestSolution.Clone();
                    }

                    // Perform a random walk.
                    RandomWalk();
                    _evalScore = InitHeuristics(ActiveSudoku);
                    _localMax = _evalScore;

                    consecutiveIterationsWithoutImprovement = 0;
                    _walksEntered++;
                }
            }

            // Print relevant information to the console.
            Print();

            _iterations++;
        }

        ShowFinalResult();
    }

    /// <summary>
    /// Determines the best successor of a current <see cref="Sudoku"/> state.
    /// </summary>
    private (Sudoku, ushort) DetermineBestSuccessor() {
        (Sudoku, ushort) bestSuccessor = new(ActiveSudoku, ushort.MaxValue);
        (ushort, ushort) c1 = new(), c2 = new();

        // Randomly select a cluster.
        ushort clusterIndex = (ushort)_random.Next(0, 9);

        HashSet<(ushort, ushort)> invalidPositions = ActiveSudoku.GetSudokuGrid()[clusterIndex].RetrieveInvalidCells();

        for (int i = 0; i < invalidPositions.Count; i++) {
            for (int j = i + 1; j < invalidPositions.Count; j++) {
                Sudoku clone = (Sudoku)ActiveSudoku.Clone();
                SudokuCluster cluster = clone.GetSudokuGrid()[clusterIndex];

                // Determine two cells to swap within a cluster.
                (ushort, ushort) cell1 = invalidPositions.ElementAt(i);
                (ushort, ushort) cell2 = invalidPositions.ElementAt(j);
                cluster.SwapCells(cell1, cell2);

                /* Adjust the cell's coordinates to apply to a whole sudoku grid (9x9),
                   rather than a single cluster (3x3) */
                c1 = ((ushort)(cell1.Item1 + clusterIndex % 3 * 3), (ushort)(cell1.Item2 + clusterIndex / 3 * 3));
                c2 = ((ushort)(cell2.Item1 + clusterIndex % 3 * 3), (ushort)(cell2.Item2 + clusterIndex / 3 * 3));

                ushort tempScore = UpdateHeuristics(clone, c1, c2);

                // Only adjust the best successor if an improvement has been found.
                if (tempScore < bestSuccessor.Item2) {
                    bestSuccessor = ((Sudoku)clone.Clone(), tempScore);
                }
            }
        }

        UpdateHeuristics(bestSuccessor.Item1, c1, c2);
        return bestSuccessor;
    }

    /// <summary>
    /// Performs a random walk if the algorithm is stuck on a local maximum or plateau.
    /// </summary>
    public void RandomWalk() {
        Sudoku clone = (Sudoku)ActiveSudoku.Clone();

        for (int i = 0; i < _randomWalks; i++) {
            // Randomly select a cluster
            ushort clusterIndex = (ushort)_random.Next(0, 9);

            SudokuCluster cluster = clone.GetSudokuGrid()[clusterIndex];
            HashSet<(ushort, ushort)> nonFixedPositions = cluster.RetrieveInvalidCells();

            // Randomly select two cells to swap.
            (ushort, ushort) cell1 = nonFixedPositions.ElementAt(_random.Next(0, nonFixedPositions.Count));
            (ushort, ushort) cell2;
            do
                // Make sure the two randomly selected cells are not equal.
                cell2 = nonFixedPositions.ElementAt(_random.Next(0, nonFixedPositions.Count));
            while (cell1 == cell2);

            cluster.SwapCells(cell1, cell2);
        }

        ActiveSudoku = (Sudoku)clone.Clone();
    }

    private protected override void PrintStats() {
        /* Temporarily stop the timer to solely include computation time,
           as printing takes a considerable amount of time. */
        Timer.Stop();

        if (Timer.ElapsedMilliseconds % PrintInterval == 0) {
            Console.Clear();
            Console.WriteLine("┌───────────────────────────────┐");
            Console.WriteLine($"│ Timer: {Timer.Elapsed}\t│");
            Console.WriteLine($"│ Eval: {_evalScore}, From: {_localMax}\t\t│");
            Console.WriteLine($"│               Best: {_bestScore}\t\t│");
            Console.WriteLine($"│ Iterations: {_iterations}\t\t│");
            Console.WriteLine($"│ Random walks: {_walksEntered}\t\t│");
            Console.WriteLine("└───────────────────────────────┘");
            ActiveSudoku.Show();
        }

        Timer.Start();
    }

    private protected override void ShowFinalResult() {
        // Clear previous computations and statistics.
        Console.Clear();

        Console.WriteLine("┌───────────────────────────────┐");
        Console.WriteLine($"│ Total Time: {Timer.Elapsed}\t│");
        Console.WriteLine($"│ Best Score: {_bestScore}\t\t\t│");
        Console.WriteLine($"│ Iterations: {_iterations}\t\t│");
        Console.WriteLine($"│ Random walks: {_walksEntered}\t\t│");
        Console.WriteLine("└───────────────────────────────┘");

        ActiveSudoku.Show();
    }
}
