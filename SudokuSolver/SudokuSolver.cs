using System;

namespace SudokuSolver;

public class SudokuSolver
{
    public Sudoku? _currentPuzzle;
    private ushort[] _scores = new ushort[18];

    private ushort bestScore = 0;
    private ushort RandomRestartTokens = 0;
    private ushort RandomWalkTokens = 0;
    private ushort MaxIterations = 0;
    private double BiasedProbabilty = 1;


    public SudokuSolver(Sudoku puzzle, ushort _RandomRestartTokens, ushort _RandomWalkTokens, ushort _MaxIterations, double _BiasedProbabilty)
    {
        // Initialize currentGrid with the first Sudoku grid
        _currentPuzzle = puzzle;

        // Initialize algorithm params  
        RandomRestartTokens = _RandomRestartTokens;
        RandomWalkTokens = _RandomWalkTokens;
        MaxIterations = _MaxIterations;
        BiasedProbabilty = _BiasedProbabilty;

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

                ushort tempScore = InitializeSudokuScore(); //UpdateHeuristicScore(cell1, cell2); 
                successors.Add((clone, tempScore));
                cluster.SwapCells(cell1, cell2);
            }
        }

        // Order successors by improved score (ascending order)
        successors = successors.OrderBy(successor => successor.Item2).ToList();

        return successors;
    }

    public void Hillclimb()
    {
        int restarts = 0;
        while (restarts < RandomRestartTokens)
        {
            Console.WriteLine($"\nRestart #{restarts + 1}");
            // Generate a random initial state
            _currentPuzzle.FillAllMissingValues();
            _currentPuzzle.Print();
            int localMaximum = 0;
            int iterations = 0;
            // Apply hill climbing to the current random initial state
            while (iterations < MaxIterations)
            {
       
                    List<(Sudoku, ushort)> successors = GetSuccessorsOrderedByScore();
                    // Biased random-walk: Select the best successor with probability p, otherwise select a random successor
                    Sudoku temp;
                    ushort newScore;
                    if (new Random().NextDouble() < BiasedProbabilty && successors.Count > 0)
                    {
                        (temp, newScore) = successors.First();  // Select the best successor
                    }
                    else
                    {
                        int randomIndex = new Random().Next(successors.Count);
                        (temp, newScore) = successors[randomIndex];  // Select a random successor
                    }
                    if (newScore < bestScore)
                    {
                        bestScore = newScore;
                        _currentPuzzle = temp; // Use the current grid directly
                        localMaximum = 0;
                        _currentPuzzle.Print();
                        Console.WriteLine($"{bestScore} NEW BEST");
                    }
                    //On a local maximum
                    else if (newScore >= bestScore)
                    {
                        localMaximum++;
                        if (localMaximum < 1)
                        {
                            Console.WriteLine("LocalMax");
                        }
                    }
                    iterations++;
         
            }
            restarts++;
        }
    }

}
