using System;
using System.Data;
using System.Diagnostics;

namespace SudokuSolver;

public class SudokuSolver
{
    public Sudoku? _currentPuzzle;
    private ushort[] _scores = new ushort[18];

    private ushort bestScore = 0;
    private ushort RandomRestartTokens = 0;
    private ushort RandomWalkTokens = 3;
    private int MaxIterations = 200000;
    private double BiasedProbabilty = 1;
    private ushort LocalMaxTokens = 50;
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
            _scores[i] = Evaluate(_currentPuzzle.GetRowValues(i));
            _scores[9 + i] = Evaluate(_currentPuzzle.GetColumnValues(i));
        }
        return GetHeuristicScore();
    }

    /// <summary>
    /// Evaluate the score for an array of values
    /// </summary>
    private ushort Evaluate(ushort[] values)
    {
        HashSet<ushort> uniqueValues = new HashSet<ushort>(values.Where(v => v != 0)); // Filter out zeros

        return (ushort)(9 - uniqueValues.Count);
    }

    /// <summary>
    /// Adds all the _scores values
    /// </summary>
    /// <returns>Returns the heuristics Score for the sudoku board</returns>
    public ushort GetHeuristicScore()
    {
        ushort score = 0;
        for (int i = 0; i < _scores.Length; i++)
        {
            score += _scores[i];
        }
        return score;
    }

    /// <summary>
    /// Given a row and column update the scores list
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <returns>Updated sudoku game heuristic score</returns>
    public ushort UpdateHeuristicScore((ushort row, ushort column) coord1, (ushort row, ushort column) coord2)
    {
        _scores[coord1.row] = Evaluate(_currentPuzzle.GetRowValues(coord1.row));
        _scores[coord1.column + 9] = Evaluate(_currentPuzzle.GetRowValues(coord1.column));
        _scores[coord2.row] = Evaluate(_currentPuzzle.GetRowValues(coord2.row));
        _scores[coord2.column + 9] = Evaluate(_currentPuzzle.GetRowValues(coord2.column));

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

        HashSet<(ushort, ushort)> nonFixedPositions = _currentPuzzle.GetClusters()[clusterIndex].RetrieveInvalidCells();

        for (int i = 0; i < nonFixedPositions.Count; i++)
        {
            for (int j = i + 1; j < nonFixedPositions.Count; j++)
            {
                Sudoku clone = (Sudoku)_currentPuzzle!.Clone();
                SudokuCluster cluster = clone.GetClusters()[clusterIndex];

                (ushort, ushort) cell1 = nonFixedPositions.ElementAt(i);
                (ushort, ushort) cell2 = nonFixedPositions.ElementAt(j);

                cluster.SwapCells(cell1, cell2);

                ushort tempScore = UpdateHeuristicScore(cell1, cell2); 
                successors.Add((clone, tempScore));
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
            // Randomly select a cluster
            ushort clusterIndex = (ushort)random.Next(0, 9);

            
            SudokuCluster cluster = clone.GetClusters()[clusterIndex];
            HashSet<(ushort, ushort)> nonFixedPositions = cluster.RetrieveInvalidCells();

            (ushort, ushort) cell1 = nonFixedPositions.ElementAt(random.Next(0, nonFixedPositions.Count));
            (ushort, ushort) cell2 = nonFixedPositions.ElementAt(random.Next(0, nonFixedPositions.Count));

            cluster.SwapCells(cell1, cell2);
        }
        _currentPuzzle = clone;
    }

    /// <summary>
    /// Solves the sudoku using the Random Restart Hill Climbing algorithm
    /// </summary>
    ///
    public void HillClimbing()
    {
        int consecutiveIterationsWithoutImprovement = 0;
        int iterations = 0;
        int randomWalkCounter = 0;
        Sudoku currentBestSolution = _currentPuzzle;
        ushort tempBestScore = bestScore;

        while (bestScore != 0 && iterations < MaxIterations)
        {
            // Generate the successors ordered by score
            List<(Sudoku, ushort)> successors = GetSuccessorsOrderedByScore();

            if (successors[0].Item2 < tempBestScore)
            {
                // Update the current sudoku with the best successor
                _currentPuzzle = successors[0].Item1;
                currentBestSolution = successors[0].Item1;
                // Update the score
                tempBestScore = successors[0].Item2;

                if(tempBestScore < bestScore)
                {
                    bestScore = tempBestScore;
                    currentBestSolution = _currentPuzzle;
                }

                int startX = Console.CursorLeft;
                int startY = Console.CursorTop;
                Console.WriteLine($"Current local score: {tempBestScore}                                     ");
                Console.WriteLine($"Random walks: {randomWalkCounter}: global best score: {bestScore} ");

                _currentPuzzle.Print();
                Console.SetCursorPosition(startX, startY);
            }
            //When on a local maximum or a plateau
            else
            {
                // No improvement in score
                consecutiveIterationsWithoutImprovement++;

                // Check if it's time to initiate a random walk
                if (consecutiveIterationsWithoutImprovement >= LocalMaxTokens)
                {
                    // Perform a random walk
                    RandomWalk();
                    tempBestScore = InitializeSudokuScore();
                    consecutiveIterationsWithoutImprovement = 0; // Reset the counter
                    randomWalkCounter++;
                }
            }
            iterations++;
        }
        Console.WriteLine($"Best found Score: {bestScore}                             ");
        currentBestSolution!.Print();
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