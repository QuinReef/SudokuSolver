using System.Diagnostics;

namespace SudokuSolver;

public class SudokuSolver {
    // The current state of the sudoku grid.
    private Sudoku _activeSudoku;
    // The heuristicvalues of each row and column.
    private ushort[] _heuristicScores = new ushort[18];

    private ushort RandomWalkTokens = 3;
    private ushort LocalMaxTokens = 40;

    // Statistics to print to the console.
    private ushort _bestScore;
    private ushort _iterations;
    private ushort _walksEntered;

    // Timer to measure total time complexity.
    private Stopwatch _timer = new Stopwatch();

    private readonly Random random = new Random();

    public SudokuSolver(Sudoku s, ushort walks)
    {
        // Initialize currentGrid with the first Sudoku grid
        _activeSudoku = s;

        RandomWalkTokens = walks;

        _timer.Start();

        _bestScore = InitHeuristicScore(s);
        // Console.WriteLine($"Initial score: {bestScore}");
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
    public ushort InitHeuristicScore2(Sudoku sudoku) {
        ushort score = 0;

        for (ushort i = 0; i < 9; i++) {
            // Separate computation to reduce complexity.
            ushort column = Evaluate(sudoku.GetColumnValues(i));
            ushort row = Evaluate(sudoku.GetRowValues(i));

            
            _heuristicScores[i] = column;
            _heuristicScores[9 + i] = row;

            score += (ushort)(column + row);
        }

        return score;
    }
    /// <summary>
    /// Initialises the initial heuristic values.
    /// </summary>
    public ushort InitHeuristicScore(Sudoku sudoku)
    {
        ushort score = 0;

        for (ushort i = 0; i < 9; i++)
        {
            // Separate computation to reduce complexity.
            ushort[] columValues = sudoku.GetColumnValues(i);
            ushort[] rowValues = sudoku.GetRowValues(i);

            HashSet<ushort> columnUniques = columValues.ToHashSet();
            score += (ushort)(9 - columnUniques.Count);

            HashSet<ushort> rowUniques = rowValues.ToHashSet();
            score += (ushort)(9 - rowUniques.Count);
        }

        return score;
    }

    /// <summary>
    /// Given a row and column update the scores list
    /// </summary>
    /// <returns>Updated sudoku game heuristic score</returns>
    public ushort UpdateHeuristicScore((ushort column, ushort row) coord1, (ushort column, ushort row) coord2) {
        // (8,4) | (7,5)

        // First value
        _heuristicScores[coord1.column] = Evaluate(_activeSudoku.GetColumnValues(coord1.column));
        _heuristicScores[coord1.row + 9] = Evaluate(_activeSudoku.GetRowValues(coord1.row));
        // Second value
        _heuristicScores[coord2.column] = Evaluate(_activeSudoku.GetColumnValues(coord2.column));
        _heuristicScores[coord2.row + 9] = Evaluate(_activeSudoku.GetRowValues(coord2.row));
        
        // Calculate the update heuristic score of all rows and columns.
        return GetHeuristicScore();
    }

    /// <summary>
    /// Computes the sum of all heuristic values.
    /// </summary>
    public ushort GetHeuristicScore() {
        ushort score = 0;

        for (ushort i = 0; i < _heuristicScores.Length; i++) {
            score += _heuristicScores[i];
        }

        return score;
    }

    /// <summary>
    /// Generates the succesors ordered by ascending heuristic value
    /// </summary>
    /// <returns></returns>
    public List<(Sudoku, ushort)> GetSuccessorsOrderedByScore() {
        List<(Sudoku, ushort)> successors = new List<(Sudoku, ushort)>();

        // Randomly select a cluster
        ushort clusterIndex = (ushort)new Random().Next(0, 9);

        HashSet<(ushort, ushort)> nonFixedPositions = _activeSudoku.GetSudokuGrid()[clusterIndex].RetrieveInvalidCells();

        for (int i = 0; i < nonFixedPositions.Count; i++)
        {
            for (int j = i + 1; j < nonFixedPositions.Count; j++)
            {
                Sudoku clone = (Sudoku)_activeSudoku!.Clone();
                SudokuCluster cluster = clone.GetSudokuGrid()[clusterIndex];

                (ushort, ushort) cell1 = nonFixedPositions.ElementAt(i);
                (ushort, ushort) cell2 = nonFixedPositions.ElementAt(j);

                cluster.SwapCells(cell1, cell2);

                _activeSudoku = (Sudoku)clone.Clone();

                // column, row -> 5
                (ushort, ushort) c1 = ((ushort)(cell1.Item1 + clusterIndex % 3 * 3), (ushort)(cell1.Item2 + (clusterIndex / 3) * 3));
                (ushort, ushort) c2 = ((ushort)(cell2.Item1 + clusterIndex % 3 * 3), (ushort)(cell2.Item2 + (clusterIndex / 3) * 3));

                Sudoku s = ((Sudoku)clone.Clone());

                ushort tmpScore = UpdateHeuristicScore(c1, c2);
                ushort tempScore = InitHeuristicScore(s);
                successors.Add((s, tempScore));
                
            }
        }

        // Order successors by improved score (ascending order)
        successors = successors.OrderBy(successor => successor.Item2).ToList();


        return successors;
    }

    


    /// <summary>
    /// Prints the statistics for the hillclimb algorithm during runtime
    /// </summary>-
    public void PrintHillClimbStats(ushort eval, ushort localMax) {
        //int startX = Console.CursorLeft;
        //int startY = Console.CursorTop;
        Console.Clear();
        Console.WriteLine("┌───────────────────────────────┐");
        Console.WriteLine($"│ Timer: {_timer.Elapsed}\t│");
        Console.WriteLine($"│ Eval: {eval}, From: {localMax}\t\t│");
        Console.WriteLine($"│               Best: {_bestScore}\t\t│");
        Console.WriteLine($"│ Iterations: {_iterations}\t\t│");
        Console.WriteLine($"│ Random walks: {_walksEntered}\t\t│");
        Console.WriteLine($"│ Total walk steps: {_walksEntered * RandomWalkTokens}\t│");
        Console.WriteLine("└───────────────────────────────┘");

        _activeSudoku.Show();
        //Console.SetCursorPosition(startX, startY);
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
            List<(Sudoku, ushort)> successors = GetSuccessorsOrderedByScore();

            if (successors[0].Item2 < tempBestScore) {
                // Update the current sudoku with the best successor
                _activeSudoku = (Sudoku)successors[0].Item1.Clone();

                // Update the score
                tempBestScore = successors[0].Item2;

                //If local solution is best
                if (tempBestScore < _bestScore)
                {
                    _bestScore = tempBestScore;
                    currentBestSolution = (Sudoku)_activeSudoku.Clone();
                }

                // Show the current state of the sudoku grid every 100 iterations.
                
            }

            //When on a local maximum or a plateau
            else {
                // No improvement in score
                consecutiveIterationsWithoutImprovement++;

                // Check if it's time to initiate a random walk
                if (consecutiveIterationsWithoutImprovement >= LocalMaxTokens)
                {
                    //Did the random walk have a higher local optimum, return to the previous local maximum
                    if (tempBestScore > _bestScore)
                    {
                        
                        _activeSudoku = (Sudoku)currentBestSolution!.Clone(); ;
                    }
                    // Perform a random walk
                    RandomWalk();
                    // Console.WriteLine(InitHeuristicScore());
                    tempBestScore = InitHeuristicScore(_activeSudoku);
                    localMax = tempBestScore;
                    // Console.WriteLine(ttempBestScore);

                    // Console.WriteLine(ttempBestScore);
                    consecutiveIterationsWithoutImprovement = 0; // Reset the counter
                    _walksEntered++;
                }
            }
            if (_iterations % 5000 == 0)
            {
                /* Temporarily stop the timer while printing the current state,
                   because we do not wish to count that towards the time complexity. */
                _timer.Stop();
                PrintHillClimbStats(tempBestScore, localMax);
                Console.WriteLine();
                currentBestSolution.Show();
                _timer.Start();
            }

            _iterations++;
        }

        Console.WriteLine($"Best found Score: {_bestScore}                             ");
        currentBestSolution!.Show();
    }

    public void RandomWalk() {
        //Implement a random walk of LocalMaxTokens steps
        ushort randomWalkScore = InitHeuristicScore(_activeSudoku);

        Sudoku clone = (Sudoku)_activeSudoku.Clone();

        randomWalkScore = InitHeuristicScore(clone);

        for (int i = 0; i < RandomWalkTokens; i++) {
            //// Randomly select a cluster
            ushort clusterIndex = (ushort)random.Next(0, 9);

            SudokuCluster cluster = clone.GetSudokuGrid()[clusterIndex];
            HashSet<(ushort, ushort)> nonFixedPositions = cluster.RetrieveInvalidCells();

            (ushort, ushort) cell1 = nonFixedPositions.ElementAt(random.Next(0, nonFixedPositions.Count));
            (ushort, ushort) cell2 = nonFixedPositions.ElementAt(random.Next(0, nonFixedPositions.Count));

            cluster.SwapCells(cell1, cell2);

            // List<(Sudoku, ushort)> successors = GetSuccessorsOrderedByScore();
            //
            // _activeSudoku = (Sudoku)successors[random.Next(0, successors.Count)].Item1.Clone();
        }

        _activeSudoku = (Sudoku)clone.Clone();

        randomWalkScore = InitHeuristicScore(_activeSudoku);
    }
}
