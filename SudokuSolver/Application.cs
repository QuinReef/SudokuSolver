//using static Sudoku.SudokuSolver;

//using static Sudoku.SudokuSolver;

using static Sudoku.SudokuSolver;

namespace SudokuSolver; 

public class Application {
    public static void Main(string[] args) {
        // Retrieve the sudoku to solve from the user.
        int? grid = SelectGrid();
        while (grid == null) {
            grid = SelectGrid();
        }

        // Read the selected sudoku puzzle from the input file.
        string puzzle = ReadInputFile(grid);

        // Load the selected sudoku, and execute the program logic.
        Sudoku sudoku = new Sudoku(puzzle);
        sudoku.Load();
        sudoku.Print();
        SudokuSolver sudokuSolver = new SudokuSolver(sudoku, 5, 100, 10000, 0.8);
        sudoku.FillAllMissingValues();
        sudoku.Print();
        sudokuSolver.InitializeSudokuScore();
        sudokuSolver.GetHeuristicScore();
        Console.ReadLine();
    }

    private static int? SelectGrid() {
        Console.Write("Please select a Sudoku grid between 1 and 5: ");
        string input = Console.ReadLine()!;

        if (!int.TryParse(input, out int grid) || grid is < 1 or > 5) {
            Console.WriteLine("Invalid grid input.", Console.ForegroundColor = ConsoleColor.Red);
            Console.ResetColor();
            return null;
        }

        return grid;
    }

    private static string ReadInputFile(int? grid) {
        string[] lines = File.ReadAllLines("../../../sudoku_input.txt");

        return lines[(int)(grid - 1)! * 2 + 1];
    }
}
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
        HashSet<ushort> uniqueValues = new HashSet<ushort>(values);

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
    public ushort updateHeuristicScore( (ushort row, ushort column) coord1, (ushort row, ushort column) coord2)
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
    private List<(Sudoku, int)> GetSuccessorsOrderedByScore()
    {
        List<(Sudoku, int)> successors = new List<(Sudoku, int)>();

        // Randomly select a cluster
        ushort clusterIndex = (ushort) new Random().Next(0, 9);

        SudokuCluster cluster = _currentPuzzle.GetCluster(clusterIndex);

        HashSet<(ushort, ushort)> nonFixedPositions = cluster.RetrieveInvalidCells();

        for (int i = 0; i < nonFixedPositions.Count; i++)
        {
            for (int j = i + 1; j < nonFixedPositions.Count; j++)
            {
                var cell1 = nonFixedPositions.ElementAt(i);
                var cell2 = nonFixedPositions.ElementAt(j);

                cluster.SwapCells(cell1, cell2);

                //int tempScore = updateHeuristicScore(cell1, cell2); 
                //successors.Add((newPuzzle, tempScore));
            }
        }

        // Order successors by improved score (ascending order)
        successors = successors.OrderBy(successor => successor.Item2).ToList();

        return successors;
    }


}
public class Sudoku {
    private SudokuCluster[] _clusters = new SudokuCluster[9];

    private string _grid;

    public Sudoku(string input) {
        _grid = input;
    }

    public SudokuCluster GetCluster(ushort index) => _clusters[index];


    /// <summary>
    /// Given an input string, loads a sudoku board game
    /// </summary>
    public void Load() {
        try
        {
            // Convert the input into a list of ushorts, skipping the initial white space.
            ushort[] values = _grid.Split(' ').Skip(1).Select(ushort.Parse).ToArray(); // O(n)

            for (ushort i = 0; i < 3; i++)
            {
                SudokuCluster cluster1 = new(), cluster2 = new(), cluster3 = new();

                for (ushort row = 0; row < 3; row++)
                {
                    for (ushort j = 0; j < 9; j++)
                    {
                        ushort value = values[i * 27 + row * 9 + j];

                        // Determine the cluster.
                        switch (j / 3)
                        {
                            case 0:
                                cluster1.AddCell((j, row), value);

                                if (value == 0)
                                {
                                    cluster1.AddInvalidCell((j, row));
                                }
                                else
                                {
                                    cluster1.AddFixedPosition((j, row));
                                    cluster1.RemoveAvailableDigit(value);
                                }

                                break;
                            case 1:
                                cluster2.AddCell(((ushort)(j - 3), row), value);

                                if (value == 0)
                                {
                                    cluster2.AddInvalidCell(((ushort)(j - 3), row));
                                }
                                else
                                {
                                    cluster2.AddFixedPosition(((ushort)(j - 3), row));
                                    cluster2.RemoveAvailableDigit(value);
                                }
                                break;
                            case 2:
                                cluster3.AddCell(((ushort)(j - 6), row), value);

                                if (value == 0)
                                {
                                    cluster3.AddInvalidCell(((ushort)(j - 6), row));
                                }
                                else
                                {
                                    cluster3.AddFixedPosition(((ushort)(j - 6), row));
                                    cluster3.RemoveAvailableDigit(value);

                                }

                                break;
                        }
                    }
                }

                _clusters[i * 3] = cluster1;
                _clusters[i * 3 + 1] = cluster2;
                _clusters[i * 3 + 2] = cluster3;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error reading Sudoku puzzle from file: " + ex.Message);
        }
    }

    /// <summary>
    /// Presents the current state of the sudoku puzzle in a proper format on the console interface.
    /// </summary>
    public void Print() {
        Console.WriteLine("\n┌───────┬───────┬───────┐");

        for (int i = 0; i < 3; i++)
        {
            for (int row = 0; row < 3; row++) 
            {
                Console.Write("│ ");

                for (int j = 0; j < 9; j++)
                {
                    Console.Write($"{_clusters[i * 3 + j / 3].RetrieveCells()[(ushort)(j % 3), row]} ");

                    if ((j + 1) % 3 == 0) {
                        Console.Write("│ ");
                    }
                }

                Console.WriteLine();
            }

            if (i < 2) {
                Console.WriteLine("├───────┼───────┼───────┤");
            }
        }

        Console.WriteLine("└───────┴───────┴───────┘");
    }

    /// <summary>
    /// Fills the Sudoku puzzle randomly.
    /// </summary>
    public void FillAllMissingValues()
    {
        // Fill each grid in the 3x3 puzzle
        for (int i = 0; i < 9; i++)
        {
            _clusters[i] = _clusters[i].FillMissingValues();
        }
    }

    /// <summary>
    /// Given a row index, return the suduko values corresponding with the column
    /// </summary>
    public ushort[] GetRowValues(ushort row)
    {
        ushort[] rowValues = new ushort[9];

        for (int j = 0; j < 9; j++)
        {
            rowValues[j] = _clusters[row / 3 * 3 + j / 3].RetrieveCells()[(ushort)(j % 3), row % 3];
        }

        return rowValues;
    }

    /// <summary>
    /// Given a column index, return the suduko values corresponding with the column
    /// </summary>
    public ushort[] GetColumnValues(ushort column)
    {
        ushort[] columnValues = new ushort[9];

        for (int i = 0; i < 3; i++)
        {
            columnValues[i * 3] = _clusters[i + column / 3 * 3].RetrieveCells()[(ushort)(column % 3), 0];
            columnValues[i * 3 + 1] = _clusters[i + column / 3 * 3].RetrieveCells()[(ushort)(column % 3), 1];
            columnValues[i * 3 + 2] = _clusters[i + column / 3 * 3].RetrieveCells()[(ushort)(column % 3), 2];
        }

        return columnValues;
    }

}
