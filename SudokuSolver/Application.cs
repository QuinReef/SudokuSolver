using SudokuSolver.SudokuSolvers;

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

        // Determine which algorithm to solve the Sudoku with.
        int? algorithm = SelectUserInput(4, "Please select the preferred algorithm (1: HC, 2: CTB, 3: FC, 4: FC-MCV): ");

        // Retrieve the sudoku to solve from the user.
        int limit = _sudokuInput!.Count / 2; // the sudoku should always be on the second line
        int? grid = SelectUserInput(limit, $"Please select a Sudoku grid between 1 and {limit}: ");

        // Read the selected sudoku puzzle from the input file.
        string input = SelectSudoku(grid);

        /* Determine whether to show the calculations during execution,
           or to solely solve the sudoku, only printing the final result. */
        bool showSteps = DetermineOutput();

        // Clear user input.
        Console.Clear();

        // Load the selected sudoku, and execute the program logic.
        Sudoku sudoku = new(input);
        SolveSudoku(sudoku, algorithm, showSteps);
 
        /* Uncomment the two lines below, and comment the two lines above,
           to start running the experiment class. */

        //Experiment experiment = new();
        //experiment.TestSudokuWalkSize(sudoku);
    }

    private static void ReadInputFile(string filePath) {
        // Read the input from the original file.
        string[] lines = File.ReadAllLines(filePath);

        // Exclude comments within a code block, as well as empty lines.
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

    /// <summary>
    /// Retrieve input from the user for any specific setting returning an Integer value.
    /// It will enter recursion if invalid input was provided to prevent any errors.
    /// </summary>
    /// <param name="limit">The maximum value of the input.</param>
    /// <param name="message">The message to show the user, requesting their input.</param>
    /// <returns>The valid input provided by the user.</returns>
    private static int? SelectUserInput(int limit, string message) {
        Console.Write(message);
        string input = Console.ReadLine()!;

        // Enter recursion if the input is not valid.
        if (!int.TryParse(input, out int output) || output < 1 || output > limit) {
            Console.WriteLine("Invalid input.", Console.ForegroundColor = ConsoleColor.Red);
            Console.ResetColor();
            return SelectUserInput(limit, message);
        }

        return output;
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

    private static string SelectSudoku(int? grid) {
        // The input file will have the appropriate sudoku input on each second line.
        int index = (int)grid! * 2 - 1;
        return _sudokuInput![index];
    }

    // Loads the appropriate solving algorithm based on the provided algorithm choice.
    private static void SolveSudoku(Sudoku sudoku, int? input, bool showSteps) {
        switch (input) {
            case 1:
                // Solve the sudoku using Iterated Local Search.
                sudoku.Load(true);
                SudokuSolverHC solverHC = new(sudoku, 5, showSteps);
                solverHC.HillClimbing();
                break;
            case 2:
                // Solve the sudoku using Chronological BackTracking.
                sudoku.Load(false);
                SudokuSolverCBT solverCBT = new(sudoku, showSteps,50);
                solverCBT.Solve();
                break;
            case 3:
                // Solve the sudoku using Forward-Checking.
                sudoku.Load(false);
                SudokuSolverFC solverFC = new(sudoku, showSteps,50);
                solverFC.Solve();
                break;
            case 4:
                // Solve the sudoku using Forward-Checking with a Most-Constrained-Variable.
                sudoku.Load(false);
                SudokuSolverFCMCV solverFCMCV = new(sudoku, showSteps,50);
                solverFCMCV.Solve();
                break;
            // Input is already restrained, so does not require a default case.
        }
    }
}
