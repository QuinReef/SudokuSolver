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

public class Sudoku {
    private SudokuCluster[] _clusters = new SudokuCluster[9];

    private string _grid;

    public Sudoku(string input) {
        _grid = input;
    }

    public void Load() {
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
                    switch (j / 3) {
                        case 0:
                            cluster1.AddCell((j, row), value);

                            if (value == 0) {
                                cluster1.AddInvalidCell((j, row));
                            }

                            break;
                        case 1:
                            cluster2.AddCell(((ushort)(j - 3), row), value);

                            if (value == 0) {
                                cluster2.AddInvalidCell(((ushort)(j - 3), row));
                            }

                            break;
                        case 2:
                            cluster3.AddCell(((ushort)(j - 6), row), value);

                            if (value == 0) {
                                cluster3.AddInvalidCell(((ushort)(j - 6), row));
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
}
