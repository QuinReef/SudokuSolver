using System;
using System.Data;
using System.Diagnostics;

namespace SudokuSolver;

public class SudokuSolver
{
    public Sudoku? _currentPuzzle;
    // The heuristicvalues of each row and column.
    private ushort[] _heuristicScores = new ushort[18];

    private ushort bestScore = 0;
    private ushort RandomRestartTokens = 0;
    private ushort RandomWalkTokens = 5;
    private int MaxIterations = 200000;
    private double BiasedProbabilty = 1;
    private ushort LocalMaxTokens = 9;
    private readonly Random random = new Random();
    public SudokuSolver(Sudoku puzzle, ushort _RandomRestartTokens, ushort _RandomWalkTokens, int _MaxIterations, double _BiasedProbabilty)
    {
        // Initialize currentGrid with the first Sudoku grid
        _currentPuzzle = puzzle;

        //Initialize algorithm params  
        //RandomRestartTokens = _RandomRestartTokens;
        //RandomWalkTokens = _RandomWalkTokens;
        //MaxIterations = _MaxIterations;
        //BiasedProbabilty = _BiasedProbabilty;

        bestScore = InitializeSudokuScore();
        Console.WriteLine($"Initial score: {bestScore}");
    }

    /// <summary>
    /// Gets the initial sudoku score 
    /// </summary>
    public ushort InitializeSudokuScore()
    {
        for (ushort i = 0; i < 9; i++)
        {
            _heuristicScores[i] = Evaluate(_currentPuzzle!.GetRowValues(i));
            _heuristicScores[9 + i] = Evaluate(_currentPuzzle.GetColumnValues(i));
        }
        return GetHeuristicScore();
    }

    /// <summary>
    /// Evaluate the score for an array of values
    /// </summary>
    private ushort Evaluate(ushort[] values) {
        SortedSet<ushort> uniques = new SortedSet<ushort>();
        
        ushort counter = 0;
        foreach (ushort val in values) {
            if (!uniques.Add(val)) {
                counter++;
            }
        }
        
        return counter;
    }

    /// <summary>
    /// Adds all the _heuristicScores values
    /// </summary>
    /// <returns>Returns the heuristics Score for the sudoku board</returns>
    public ushort GetHeuristicScore()
    {
        ushort score = 0;
        for (int i = 0; i < _heuristicScores.Length; i++)
        {
            score += _heuristicScores[i];
        }

        return score;
    }

    /// <summary>
    /// Given a row and column update the scores list
    /// </summary>
    /// <returns>Updated sudoku game heuristic score</returns>
    public ushort UpdateHeuristicScore((ushort column, ushort row) coord1, (ushort column, ushort row) coord2) {
        // (0,7) -> (1,8)

        _heuristicScores[coord1.column] = Evaluate(_currentPuzzle!.GetColumnValues(coord1.column));
        _heuristicScores[coord1.row + 9] = Evaluate(_currentPuzzle.GetRowValues(coord1.row));

        _heuristicScores[coord2.column] = Evaluate(_currentPuzzle.GetColumnValues(coord2.column));
        _heuristicScores[coord1.row + 9] = Evaluate(_currentPuzzle.GetRowValues(coord2.row));

        return GetHeuristicScore();
    }

    /// <summary>
    /// Generates the succesors ordered by ascending heuristic value
    /// </summary>
    /// <returns></returns>
    public List<(Sudoku, ushort)> GetSuccessorsOrderedByScore()
    {
        List<(Sudoku, ushort)> successors = new List<(Sudoku, ushort)>();

        // Randomly select a cluster
        ushort clusterIndex = (ushort)new Random().Next(0, 9);

        HashSet<(ushort, ushort)> nonFixedPositions = _currentPuzzle!.GetSudokuGrid()[clusterIndex].RetrieveInvalidCells();

        for (int i = 0; i < nonFixedPositions.Count; i++)
        {
            for (int j = i + 1; j < nonFixedPositions.Count; j++)
            {
                Sudoku clone = (Sudoku)_currentPuzzle!.Clone();
                Sudoku old = (Sudoku)_currentPuzzle.Clone();
                SudokuCluster cluster = clone.GetSudokuGrid()[clusterIndex];

                (ushort, ushort) cell1 = nonFixedPositions.ElementAt(i);
                (ushort, ushort) cell2 = nonFixedPositions.ElementAt(j);

                cluster.SwapCells(cell1, cell2);

                _currentPuzzle = (Sudoku)clone.Clone();

                // column, row -> 5
                (ushort, ushort) c1 = ((ushort)(cell1.Item1 + clusterIndex % 3 * 3), (ushort)(cell1.Item2 + clusterIndex / 3 * 3));
                (ushort, ushort) c2 = ((ushort)(cell2.Item1 + clusterIndex % 3 * 3), (ushort)(cell2.Item2 + clusterIndex / 3 * 3));

                ushort tempScore = UpdateHeuristicScore(c1, c2); 
                successors.Add((clone, tempScore));

                _currentPuzzle = (Sudoku)old.Clone();
            }
        }

        // Order successors by improved score (ascending order)
        successors = successors.OrderBy(successor => successor.Item2).ToList();

        return successors;
    }

    public void RandomWalk() {
        //Implement a random walk of LocalMaxTokens steps
        Sudoku clone = (Sudoku)_currentPuzzle!.Clone();

        for (int i = 0; i < RandomWalkTokens; i++)
        {
            //// Randomly select a cluster
            //ushort clusterIndex = (ushort)random.Next(0, 9);

            
            //SudokuCluster cluster = clone.GetClusters()[clusterIndex];
            //HashSet<(ushort, ushort)> nonFixedPositions = cluster.RetrieveInvalidCells();

            //(ushort, ushort) cell1 = nonFixedPositions.ElementAt(random.Next(0, nonFixedPositions.Count));
            //(ushort, ushort) cell2 = nonFixedPositions.ElementAt(random.Next(0, nonFixedPositions.Count));

            //cluster.SwapCells(cell1, cell2);

            List<(Sudoku, ushort)> successors = GetSuccessorsOrderedByScore();

            _currentPuzzle = (Sudoku)successors[random.Next(0, successors.Count)].Item1.Clone();
        }
        //_currentPuzzle = clone;
        ushort randomWalkScore = InitializeSudokuScore();
    }

    /// <summary>
    /// Prints the statistics for the hillclimb algorithm during runtime
    /// </summary>
    /// <param name="startedFrom"></param>
    /// <param name="tempBestScore"></param>
    /// <param name="randomWalkCounter"></param>
    public void PrintHillClimbStats(int startedFrom, ushort tempBestScore, int randomWalkCounter)
    {
        int startX = Console.CursorLeft;
        int startY = Console.CursorTop;
        Console.WriteLine($"┌───────────────────────────────┐");
        Console.WriteLine($"│ Started from: {startedFrom}\t\t│");
        Console.WriteLine($"│ Current local score: {tempBestScore}\t│");
        Console.WriteLine($"│ Random walks: {randomWalkCounter}\t\t│");
        Console.WriteLine($"│ Random walks steps: {randomWalkCounter * RandomWalkTokens}\t\t│");
        Console.WriteLine($"│ Global best score: {bestScore}\t\t│");
        Console.WriteLine($"└───────────────────────────────┘");

        _currentPuzzle!.Print();
        Console.SetCursorPosition(startX, startY);
    }

    /// <summary>
    /// Solves the sudoku using the Random Restart Hill Climbing algorithm
    /// </summary>
    public void HillClimbing()
    {
        //_currentPuzzle.Print();
        //Console.WriteLine(InitializeSudokuScore());

        //Sudoku newSudoku = (Sudoku)_currentPuzzle.Clone();
        //newSudoku.Print();

        //_currentPuzzle = (Sudoku)newSudoku.Clone();
        //Console.WriteLine();
        //Console.WriteLine(InitializeSudokuScore());


        int consecutiveIterationsWithoutImprovement = 0;
        int iterations = 0;
        int randomWalkCounter = 0;
        int startedFrom = 0;
        Sudoku currentBestSolution = (Sudoku)_currentPuzzle.Clone();
        ushort tempBestScore = bestScore;

        while (bestScore != 0 && iterations < MaxIterations)
        {
            // Generate the successors ordered by score
            List<(Sudoku, ushort)> successors = GetSuccessorsOrderedByScore();

            if (successors[0].Item2 < tempBestScore)
            {
                // Update the current sudoku with the best successor
                _currentPuzzle = (Sudoku) successors[0].Item1.Clone();
  
                // Update the score
                tempBestScore = successors[0].Item2;

                //If local solution is best
                if (tempBestScore < bestScore)
                {
                    bestScore = tempBestScore;
                    currentBestSolution = (Sudoku)_currentPuzzle!.Clone();
                }

                PrintHillClimbStats(startedFrom, tempBestScore, randomWalkCounter);

            }
            //When on a local maximum or a plateau
            else
            {
                // No improvement in score
                consecutiveIterationsWithoutImprovement++;

                // Check if it's time to initiate a random walk
                if (consecutiveIterationsWithoutImprovement >= LocalMaxTokens)
                {
                    //Did the random walk have a higher local optimum, return to the previous local maximum
                    if (tempBestScore > bestScore)
                    {
                        _currentPuzzle = (Sudoku)currentBestSolution!.Clone(); ;
                    }
                    ushort bestScoreTest = InitializeSudokuScore();
                    // Perform a random walk
                    RandomWalk();
                    tempBestScore = InitializeSudokuScore();
                    startedFrom = tempBestScore;
                    consecutiveIterationsWithoutImprovement = 0; // Reset the counter
                    randomWalkCounter++;
                }
            }
            iterations++;
        }
        Console.WriteLine($"Best found Score: {bestScore}                             ");
        currentBestSolution!.Print();
        //}
    }
}


            
            //// Restart the algorithm until the sudoku is solved or the max iterations are reached
            //while (bestScore != 0 && maxIterations > 0)
            //{
            //    // Generate the successors ordered by score
            //    List<(Sudoku, ushort)> successors = GetSuccessorsOrderedByScore();

//                if (random.NextDouble() > BiasedProbabilty)
//                {
//                    (Sudoku, ushort) randomSuccesor = successors[random.Next(0, successors.Count)];
//                    // Update the current sudoku with the best successor
//                    _currentPuzzle = randomSuccesor.Item1;
//                    // Update the score
//                    tempBestScore = randomSuccesor.Item2;
//                    // Decrease the number of random walk tokens
//                    int startX = Console.CursorLeft;
//                    int startY = Console.CursorTop;
//                    Console.WriteLine($"Random Chosen score: {tempBestScore}                         ");
//                    _currentPuzzle.Print();
//                    Console.SetCursorPosition(startX, startY);
//                }
//                // Check if the best successor has a better score
//                else if (successors[0].Item2 < tempBestScore)
//                {
//                    // Update the current sudoku with the best successor
//                    _currentPuzzle = successors[0].Item1;
//                    // Update the score
//                    tempBestScore = successors[0].Item2;
          
//                    int startX = Console.CursorLeft;
//                    int startY = Console.CursorTop;
//                    Console.WriteLine($"Current score: {tempBestScore}                                     ");
//                    _currentPuzzle.Print();
//                    Console.SetCursorPosition(startX, startY);
//                }
//                else
//                {
//                    // No improvement in score
//                    consecutiveIterationsWithoutImprovement++;

//                    // Check if it's time to initiate a random walk
//                    if (consecutiveIterationsWithoutImprovement >= LocalMaxTokens)
//                    {
//                        // Perform a random walk
//                        RandomWalk();
//                        consecutiveIterationsWithoutImprovement = 0; // Reset the counter
//                    }
//                }

//                iterations++;
//            }
          
//     }
   
//}