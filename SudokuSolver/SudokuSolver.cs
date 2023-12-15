using System.Diagnostics;

namespace SudokuSolver;

public class SudokuSolver {
    // The current state of the sudoku grid.
    private Sudoku _activeSudoku;
    // The heuristicvalues of each row and column.
    // public ushort[] _heuristicScores = new ushort[18];

    private ushort _randomWalks;
    private ushort _localMaxLimit = 40;

    // Statistics to print to the console.
    private ushort _bestScore;
    private int _iterations;
    private ushort _walksEntered;

    // Timer to measure total time complexity.
    private Stopwatch _timer = new();

    private readonly Random _random = new();

    public SudokuSolver(Sudoku s, ushort walks) {
        _activeSudoku = s;

        _randomWalks = walks;
        _bestScore = InitHeuristicScore(s);

        _timer.Start();
    }

    /// <summary>
    /// Evaluate the score for an array of values
    /// </summary>
    private ushort Evaluate(ushort[] values) {
        SortedSet<ushort> uniques = new SortedSet<ushort>(); // TODO: change

        ushort counter = 0;
        foreach (ushort val in values) {
            if (!uniques.Add(val)) {
                counter++;
            }
        }

        return counter;
    }

    /// <summary>
    /// Initialises the initial heuristic values.
    /// </summary>
    public ushort InitHeuristicScore(Sudoku sudoku) {
        ushort score = 0;

        for (ushort i = 0; i < 9; i++) {
            // Separate computation to reduce complexity.
            ushort column = Evaluate(sudoku.GetColumnValues(i));
            ushort row = Evaluate(sudoku.GetRowValues(i));

            sudoku.HeuristicScores[i] = column;
            sudoku.HeuristicScores[9 + i] = row;

            score += (ushort)(column + row);
        }

        return score;
    }

    /// <summary>
    /// Given a row and column update the scores list
    /// </summary>
    /// <returns>Updated sudoku game heuristic score</returns>
    public ushort UpdateHeuristicScore(Sudoku sudoku, (ushort column, ushort row) coord1, (ushort column, ushort row) coord2) {
        // First value
        sudoku.HeuristicScores[coord1.column] = Evaluate(sudoku.GetColumnValues(coord1.column));
        sudoku.HeuristicScores[coord1.row + 9] = Evaluate(sudoku.GetRowValues(coord1.row));
        // Second value
        sudoku.HeuristicScores[coord2.column] = Evaluate(sudoku.GetColumnValues(coord2.column));
        sudoku.HeuristicScores[coord2.row + 9] = Evaluate(sudoku.GetRowValues(coord2.row));
        
        // Calculate the update heuristic score of all rows and columns.
        return GetHeuristicScore(sudoku.HeuristicScores);
    }

    public ushort RetrieveHeuristicScore(Sudoku s, (ushort column, ushort row) coord1, (ushort column, ushort row) coord2) {
        ushort[] scores = new ushort[18];
        Array.Copy(s.HeuristicScores, scores, s.HeuristicScores.Length);

        // First value
        scores[coord1.column] = Evaluate(s.GetColumnValues(coord1.column));
        scores[coord1.row + 9] = Evaluate(s.GetRowValues(coord1.row));
        // Second value
        scores[coord2.column] = Evaluate(s.GetColumnValues(coord2.column));
        scores[coord2.row + 9] = Evaluate(s.GetRowValues(coord2.row));

        // Calculate the update heuristic score of all rows and columns.
        return GetHeuristicScore(scores);
    }

    /// <summary>
    /// Computes the sum of all heuristic values.
    /// </summary>
    public ushort GetHeuristicScore(ushort[] scores) {
        ushort score = 0;

        for (ushort i = 0; i < scores.Length; i++) {
            score += scores[i];
        }

        return score;
    }

    /// <summary>
    /// Generates the succesors ordered by ascending heuristic value
    /// </summary>
    /// <returns></returns>
    public (Sudoku, ushort) GetSuccessorsOrderedByScore() {
        (Sudoku sudoku, ushort currentBestFitness) bestSuccessor = new(_activeSudoku, ushort.MaxValue);

        // Randomly select a cluster
        ushort clusterIndex = (ushort)new Random().Next(0, 9);

        (ushort, ushort) ce1 = new(), ce2 = new();

        HashSet<(ushort, ushort)> nonFixedPositions = _activeSudoku.GetSudokuGrid()[clusterIndex].RetrieveInvalidCells();

        for (int i = 0; i < nonFixedPositions.Count; i++)
        {
            for (int j = i + 1; j < nonFixedPositions.Count; j++)
            {
                Sudoku clone = (Sudoku)_activeSudoku.Clone();
                SudokuCluster cluster = clone.GetSudokuGrid()[clusterIndex];

                (ushort, ushort) cell1 = nonFixedPositions.ElementAt(i);
                (ushort, ushort) cell2 = nonFixedPositions.ElementAt(j);

                cluster.SwapCells(cell1, cell2);

                (ushort, ushort) c1 = ((ushort)(cell1.Item1 + clusterIndex % 3 * 3), (ushort)(cell1.Item2 + (clusterIndex / 3) * 3));
                (ushort, ushort) c2 = ((ushort)(cell2.Item1 + clusterIndex % 3 * 3), (ushort)(cell2.Item2 + (clusterIndex / 3) * 3));

                ushort tempScore = RetrieveHeuristicScore(clone, c1, c2); // shouldn't change values

                if (tempScore < bestSuccessor.currentBestFitness) {
                    ce1 = c1; ce2 = c2;
                    bestSuccessor = ((Sudoku)clone.Clone(), tempScore);
                }
            }
        }

        UpdateHeuristicScore(bestSuccessor.sudoku, ce1, ce2);
        return bestSuccessor;
    }

    /// <summary>
    /// Prints the statistics for the hillclimb algorithm during runtime
    /// </summary>-
    public void PrintHillClimbStats(ushort eval, ushort localMax) {
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

    /// <summary>
    /// Solves the sudoku using the Random Restart Hill Climbing algorithm
    /// </summary>
    public void HillClimbing() {
        int consecutiveIterationsWithoutImprovement = 0;

        ushort tempBestScore = _bestScore;
        ushort localMax = _bestScore;
        Sudoku currentBestSolution = (Sudoku)_activeSudoku.Clone();

        while (_bestScore > 0) {
            // Generate the successors ordered by score
            (Sudoku, ushort) successor = GetSuccessorsOrderedByScore();

            // If the found successor is an improvement of the active sudoku..
            if (successor.Item2 < tempBestScore) {
                // Update the current sudoku with the best successor
                _activeSudoku = (Sudoku)successor.Item1.Clone();

                // Update the score
                tempBestScore = successor.Item2;

                //If local solution is best
                if (tempBestScore <= _bestScore) {
                    _bestScore = tempBestScore;
                    currentBestSolution = (Sudoku)_activeSudoku.Clone();
                }
            }

            //When on a local maximum or a plateau
            else {
                // No improvement in score
                consecutiveIterationsWithoutImprovement++;

                // Check if it's time to initiate a _random walk
                if (consecutiveIterationsWithoutImprovement >= _localMaxLimit)
                {
                    //Did the _random walk have a higher local optimum, return to the previous local maximum
                    if (tempBestScore > _bestScore) {
                        _activeSudoku = (Sudoku)currentBestSolution!.Clone(); ;
                    }

                    // Perform a _random walk
                    RandomWalk();
                    tempBestScore = InitHeuristicScore(_activeSudoku);
                    localMax = tempBestScore;

                    consecutiveIterationsWithoutImprovement = 0; // Reset the counter
                    _walksEntered++;
                }
            }

            if (_timer.ElapsedMilliseconds % 10 == 0) // every 0.5 second
            {
                /* Temporarily stop the timer while printing the current state,
                   because we do not wish to count that towards the time complexity. */
                _timer.Stop();
                PrintHillClimbStats(tempBestScore, localMax);
                _timer.Start();
            }

            _iterations++;
        }

        Console.WriteLine($"Best found Score: {_bestScore}                             ");
        currentBestSolution.Show();
        Console.WriteLine(GetHeuristicScore(currentBestSolution.HeuristicScores));

        foreach (ushort val in currentBestSolution.HeuristicScores) {
            Console.Write(val + " ");
        }
    }

    public void RandomWalk() {
        Sudoku clone = (Sudoku)_activeSudoku.Clone();

        for (int i = 0; i < _randomWalks; i++) {
            //// Randomly select a cluster
            ushort clusterIndex = (ushort)_random.Next(0, 9);

            SudokuCluster cluster = clone.GetSudokuGrid()[clusterIndex];
            HashSet<(ushort, ushort)> nonFixedPositions = cluster.RetrieveInvalidCells();

            // todo: 
            (ushort, ushort) cell1 = nonFixedPositions.ElementAt(_random.Next(0, nonFixedPositions.Count));
            (ushort, ushort) cell2;
            do cell2 = nonFixedPositions.ElementAt(_random.Next(0, nonFixedPositions.Count));
            while (cell1 == cell2);

            cluster.SwapCells(cell1, cell2);
        }

        _activeSudoku = (Sudoku)clone.Clone();
    }
}
