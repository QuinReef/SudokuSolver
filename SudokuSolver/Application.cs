namespace SudokuSolver;

public class Application {
    public static void Main(string[] args) {
        // Retrieve the sudoku to solve from the user.
        int? grid = SelectGrid();
        /* Determine whether to show the calculations during execution,
           or to solely solve the sudoku, only printing the final result. */
        bool showSteps = DetermineOutput();
        
        // Read the selected sudoku puzzle from the input file.
        string input = ReadInputFile(grid);
        
        // Clear user input.
        Console.Clear();
        
        // Load the selected sudoku, and execute the program logic.
        Sudoku sudoku = new(input);
        sudoku.Load();

        // Solve the sudoku using ILS.
        SudokuSolver solver = new(sudoku, 2, showSteps);
        solver.Start();


        // Experiment experiment = new();
        // experiment.TestSudokuWalkSize(sudoku);
    }

    private static int? SelectGrid() {
        Console.Write("Please select a Sudoku grid between 1 and 5: ");
        string input = Console.ReadLine()!;

        // Enter recursion if the input was not a valid sudoku index.
        if (!int.TryParse(input, out int grid) || grid is < 1 or > 5) {
            Console.WriteLine("Invalid grid input.", Console.ForegroundColor = ConsoleColor.Red);
            Console.ResetColor();
            return SelectGrid();
        }

        return grid;
    }

    private static bool DetermineOutput() {
        Console.Write("Show intermediate steps (Y/N): ");
        char input = char.ToUpper(Console.ReadKey().KeyChar);

        // Enter recursion if the input was invalid.
        if (input != 'Y' && input != 'N') {
            Console.WriteLine("Invalid option input.", Console.ForegroundColor = ConsoleColor.Red);
            Console.ResetColor();
            return DetermineOutput();
        }

        return input == 'Y';
    }

    private static string ReadInputFile(int? grid) {
        string[] lines = File.ReadAllLines("../../../sudoku_input.txt");

        // The input file will have the appropriate sudoku input on each second line.
        return lines[(int)grid! * 2 - 1];
    }
}
