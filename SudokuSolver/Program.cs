using static Sudoku.SudokuSolver;
using static Sudoku.SudokuSolver1;

namespace Sudoku;

class SudokuSolver
{
    public List<SudokuPuzzle> SudokuPuzzles = new List<SudokuPuzzle>();

    public SudokuPuzzle CurrentGrid;

    /// <summary>
    /// Represents the amount of rows and columns on a <see cref="SudokuPuzzle"/>.
    /// </summary>
    private const ushort SudokuSize = 9;
    private ushort RandomRestartTokens = 0;
    private ushort RandomWalkTokens = 0;
    private ushort MaxIterations = 0;
    private double BiasedProbabilty = 1;


    public SudokuSolver(string Path, ushort _RandomRestartTokens, ushort _RandomWalkTokens, ushort _MaxIterations, double _BiasedProbabilty)
    {
        LoadSudokuFromFile(Path);
        // Initialize currentGrid with the first Sudoku grid
        CurrentGrid = SudokuPuzzles.First();
        // Initialize algorithm params  
        RandomRestartTokens = _RandomRestartTokens;
        RandomWalkTokens = _RandomWalkTokens;
        MaxIterations = _MaxIterations;
        BiasedProbabilty = _BiasedProbabilty;
    }

    //Tuple representing a cell in the Sudoku
    public record SudokuCell(ushort Value, bool IsStatic);

    // Named 2D Array representing the Sudoku grid
    public record SudokuGrid(SudokuCell[,] Cells, List<ushort> avaibleDigits, List<(ushort,ushort)> NonFixedPositions);

    // Named 2D Array representing the Sudoku grid
    public record SudokuPuzzle(SudokuGrid[,] Grids, ushort[,] Scores);

    // Read Sudoku Puzzle from input file
    void LoadSudokuFromFile(string FilePath)
    {
        try
        {
            string[] Lines = File.ReadAllLines(FilePath);

            // Only process the lines with data
            for (int k = 1; k < Lines.Length; k += 2)
            {
                // As the first input is " ", shift the input 1 to the left 
                string[] Values = Lines[k].Split(' ').Skip(1).ToArray();

                SudokuGrid[,] Grids = new SudokuGrid[3, 3];
                SudokuPuzzle SudokuPuzzle = new SudokuPuzzle(Grids, new ushort[3,3]);

                // Counter for item in the values list.
                int valCounter = 0;
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        SudokuCell[,] grid = new SudokuCell[3, 3];
                        List<ushort> staticDigits = Enumerable.Range(1, 9).Select(x => (ushort)x).ToList();
                        List<(ushort, ushort)> nonFixedPositions = new();
                        SudokuGrid SudokuGrid = new SudokuGrid(grid, staticDigits, nonFixedPositions);
                        for (ushort i = 0; i < 3; i++)
                        {
                            for (ushort j = 0; j < 3; j++)
                            {
                                int index = x * 27 + y * 3 + i * 9 + j;

                                ushort itemVal = ushort.Parse(Values[index]);
                                bool isStatic = false;
                                if (itemVal != 0)
                                {
                                    isStatic = true;
                                    staticDigits.Remove(itemVal);
                                }
                                else
                                {
                                    nonFixedPositions.Add((i, j));

                                }
                                grid[i, j] = isStatic ? new SudokuCell(itemVal, true) : new SudokuCell(0, false);
                                valCounter++;
                            }
                        }
                        Grids[x, y] = SudokuGrid;
                    }

                }
                SudokuPuzzles.Add(SudokuPuzzle);
            }
            

        }
        catch (Exception ex)
        {
            Console.WriteLine("Error reading Sudoku puzzle from file: " + ex.Message);
        }
    }

    // Fill missing values in a 3x3 grid with random numbers
    private SudokuGrid FillMissingValues(SudokuGrid Grid)
    {
        List<ushort> tempAvaibleNumbers = Grid.avaibleDigits;
                // Fill each cell in the 3x3 grid
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (!Grid.Cells[row, col].IsStatic)
                {
                    // Choose a random number from the available numbers
                    int randomIndex = new Random().Next(tempAvaibleNumbers.Count);
                    Grid.Cells[row, col] = new SudokuCell(tempAvaibleNumbers[randomIndex], false);
                    // Remove the chosen number from the available numbers
                    tempAvaibleNumbers.RemoveAt(randomIndex);
                }
            }
        }
        return Grid;
    }

    private SudokuPuzzle FillAllMissingValues(SudokuPuzzle Puzzle)
    {
        // Fill each grid in the 3x3 puzzle
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                Puzzle.Grids[row,col] = FillMissingValues(Puzzle.Grids[row, col]);
            }
        }
        return Puzzle;
    }


    private int Evaluate(SudokuPuzzle puzzle)
    {
        int score = 0;

        // Evaluate rows
        for (int i = 0; i < 9; i++)
        {
            HashSet<ushort> rowNumbers = new HashSet<ushort>();
            HashSet<ushort> colNumbers = new HashSet<ushort>();

            for (int j = 0; j < 9; j++)
            {
                rowNumbers.Add(puzzle.Grids[i / 3, j / 3].Cells[i % 3, j % 3].Value);
                colNumbers.Add(puzzle.Grids[j / 3, i / 3].Cells[j % 3, i % 3].Value);
            }

            // Number of correct values in the row
            score += rowNumbers.Count + colNumbers.Count;
        }

        return score;
    }

    private List<(SudokuPuzzle, int)> GetSuccessorsOrderedByScore(SudokuPuzzle puzzle, int bestScore)
    {
        List<(SudokuPuzzle, int)> successors = new List<(SudokuPuzzle, int)>();

        // Randomly select a grid
        int gridX = new Random().Next(0, puzzle.Grids.GetLength(0));
        int gridY = new Random().Next(0, puzzle.Grids.GetLength(1));

        SudokuGrid grid = puzzle.Grids[gridX, gridY];

        List<(ushort, ushort)> nonFixedPositions = grid.NonFixedPositions;

        for (int i = 0; i < nonFixedPositions.Count; i++)
        {
            for (int j = i + 1; j < nonFixedPositions.Count; j++)
            {
                var cell1 = nonFixedPositions[i];
                var cell2 = nonFixedPositions[j];

                SudokuPuzzle newPuzzle = SwapCells(puzzle, gridX, gridY, cell1, cell2);

                int tempScore = Evaluate(newPuzzle);
                successors.Add((newPuzzle, tempScore));
            }
        }

        // Order successors by improved score (ascending order)
        successors = successors.OrderBy(successor => successor.Item2).ToList();

        return successors;
    }

    private SudokuPuzzle SwapCells(SudokuPuzzle puzzle, int gridX, int gridY, (int, int) cell1, (int, int) cell2)
    {
        SudokuPuzzle newPuzzle = new SudokuPuzzle(
            (SudokuGrid[,])puzzle.Grids.Clone(),
            (ushort[,])puzzle.Scores.Clone()
        );

        SudokuGrid grid = newPuzzle.Grids[gridX, gridY];

        SudokuCell temp = grid.Cells[cell1.Item1, cell1.Item2];
        grid.Cells[cell1.Item1, cell1.Item2] = grid.Cells[cell2.Item1, cell2.Item2];
        grid.Cells[cell2.Item1, cell2.Item2] = temp;

        return newPuzzle;
    }





    public void HillClimbingWithRandomRestarts()
    {
        int restarts = 0;
        int bestScore = Evaluate(CurrentGrid);

        while (restarts < RandomRestartTokens)
        {
            Console.WriteLine($"\nRestart #{restarts + 1}");

            // Generate a random initial state
            CurrentGrid = FillAllMissingValues(CurrentGrid);
            PrintGrid(CurrentGrid);

            int localMaximum = 0;
            int iterations = 0;
            int k = 0;
            // Apply hill climbing to the current random initial state
            while (iterations < MaxIterations)
            {
                List <(SudokuPuzzle, int)> successors = GetSuccessorsOrderedByScore(CurrentGrid, bestScore);

                // Biased random-walk: Select the best successor with probability p, otherwise select a random successor
                SudokuPuzzle temp;
                int newScore;
                if (new Random().NextDouble() < BiasedProbabilty && successors.Count > 0 && localMaximum > 0 && k < RandomWalkTokens)
                {
                    (temp, newScore) = successors.First();  // Select the best successor
                    k++;
                }
                else
                {
                    int randomIndex = new Random().Next(successors.Count);
                    (temp, newScore) = successors[randomIndex];  // Select a random successor
                    k = 0;
                }

                if (newScore < bestScore)
                {
                    bestScore = newScore;
                    CurrentGrid = temp; // Use the current grid directly
                    localMaximum = 0;
                    PrintGrid(temp);
                    Console.WriteLine($"{bestScore} NEW BEST");
                }
                //On a local maximum
                else if (newScore > bestScore)
                {
                    localMaximum++;
                    if (localMaximum < 3)
                    {
                        Console.WriteLine("LocalMax");
                    }

                }
                //On a plateau
                else if (newScore == bestScore)
                {
                    // This ensures the loop breaks when there is no improvement
                    localMaximum++; //Can make this its own variable to check on
                    Console.WriteLine("Plateau");
                }
                iterations++;
            }

            restarts++;
        }
    }


    /// <summary>
    /// Presents the current state of the sudoku puzzle in a proper format on the console interface.
    /// </summary>
    public static void PrintGrid(SudokuPuzzle puzzle)
    {
        Console.WriteLine("\n┌───────┬───────┬───────┐");

        for (int i = 0; i < SudokuSize; i++)
        {
            // Rows
            Console.Write("│ ");

            for (int j = 0; j < SudokuSize; j++)
            {
                // Columns
                SudokuCell cell = puzzle.Grids[i / 3, j / 3].Cells[i % 3, j % 3];

                // Give each value the appropriate color.
                Console.ForegroundColor = DetermineColor(cell);
                Console.Write(cell.Value);
                Console.ResetColor();

                // Print a column divider between each cluster, or add spacing.
                Console.Write(j % 3 == 2 ? " │ " : " ");
            }

            // Print a full divider between each row of 3x3 clusters.
            if (i % 3 == 2 && i != SudokuSize - 1)
            {
                Console.WriteLine("\n├───────┼───────┼───────┤");
            }
            else
            {
                Console.WriteLine();
            }
        }

        Console.WriteLine("└───────┴───────┴───────┘");
    }


    /// <summary>
    /// Determines the colour of a <see cref="SudokuCell"/> based on its properties.
    /// </summary>
    /// <returns>If the cell is static, returns <see cref="ConsoleColor.DarkGray"/>; otherwise, returns <see cref="ConsoleColor.White"/>.</returns>
    private static ConsoleColor DetermineColor(SudokuCell cell)
    {
        return cell.IsStatic ? ConsoleColor.DarkGray : ConsoleColor.White;
    }



}
class Program
{
    static void Main(string[] args)
    {
        ushort randomRestartTokens = 5;
        ushort randomWalkTokens = 1000;
        ushort maxIterations = 4;
        double biasedProbability = 0.7;

        SudokuSolver sv = new SudokuSolver("../../../sudoku_input.txt", randomRestartTokens, randomWalkTokens, maxIterations, biasedProbability);

        Console.WriteLine("Initial Sudoku Puzzle:");
        SudokuSolver.PrintGrid(sv.CurrentGrid);

        sv.HillClimbingWithRandomRestarts();

        Console.WriteLine("Final Sudoku Puzzle:");
        SudokuSolver.PrintGrid(sv.CurrentGrid);

        Console.ReadLine();
    }


}