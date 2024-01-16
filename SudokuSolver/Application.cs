namespace SudokuSolver;

public class Application {
    private static List<string>? _sudokuInput;

    public static void Main(string[] args) {
        while (true) {
            // Clear the console.
            Console.Clear();
            // Execute the program logic.
            Execute();
            // Wait for user input to continue.
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

    private static void Execute() {
        // Start by loading the input file.
        ReadInputFile("../../../sudoku_input.txt");

        // Retrieve the sudoku to solve from the user.
        int? grid = SelectGrid();
        /* Determine whether to show the calculations during execution,
           or to solely solve the sudoku, only printing the final result. */
        bool showSteps = DetermineOutput();

        // Read the selected sudoku puzzle from the input file.
        string input = SelectSudoku(grid);

        // Clear user input.
        Console.Clear();

        // Load the selected sudoku, and execute the program logic.
        Sudoku sudoku = new(input);
        sudoku.Load();

        // Solve the sudoku using ILS.
        SudokuSolver solver = new(sudoku, 5, showSteps);
        solver.HillClimbing();

        /* Uncomment the two lines below, and comment the two lines above,
           to start running the experiment class. */

        //Experiment experiment = new();
        //experiment.TestSudokuWalkSize(sudoku);
    }

    private static int? SelectGrid() {
        int limit = _sudokuInput!.Count / 2; // the sudoku should always be on the second line
        Console.Write($"Please select a Sudoku grid between 1 and {limit}: ");
        string input = Console.ReadLine()!;

        // Enter recursion if the input was not a valid sudoku index.
        if (!int.TryParse(input, out int grid) || grid < 1 || grid > limit) {
            Console.WriteLine("Invalid grid input.", Console.ForegroundColor = ConsoleColor.Red);
            Console.ResetColor();
            return SelectGrid();
        }

        return grid;
    }

    private static bool DetermineOutput() {
        Console.Write("Show intermediate steps (y/n): ");
        char input = char.ToUpper(Console.ReadKey().KeyChar);

        // Enter recursion if the input was invalid.
        if (input != 'Y' && input != 'N') {
            Console.WriteLine();
            Console.WriteLine("Invalid option input.", Console.ForegroundColor = ConsoleColor.Red);
            Console.ResetColor();
            return DetermineOutput();
        }

        return input == 'Y';
    }

    private static void ReadInputFile(string filePath) {
        // Read the input from the original file.
        string[] lines = File.ReadAllLines(filePath);

        // Exclude comments within a code block.
        List<string> filtered = new();
        bool insideBlockComment = false;

        foreach (string line in lines) {
            if (line.StartsWith("/*")) {
                insideBlockComment = true;
            }

            if (!insideBlockComment && line != "") {
                filtered.Add(line);
            }

            if (line.EndsWith("*/")) {
                insideBlockComment = false;
            }
        }

        _sudokuInput = filtered;
    }
    
    private static string SelectSudoku(int? grid) {
        // The input file will have the appropriate sudoku input on each second line.
        return _sudokuInput![(int)grid! * 2 - 1];
    }
}
