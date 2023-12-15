namespace SudokuSolver;

public class Application {
    public static void Main(string[] args) {
        // Retrieve the sudoku to solve from the user.
        int? grid = SelectGrid();
        bool showSteps = GetProgramDefaults();
        
        // Read the selected sudoku puzzle from the input file.
        string input = ReadInputFile(grid);
        
        // Clear user input.
        Console.Clear();
        
        // Load the selected sudoku, and execute the program logic.
        Sudoku sudoku = new(input);
        sudoku.Load();
        //SudokuSolver solver = new(sudoku, 2, showSteps);

        //solver.HillClimbing();
        Experiment experiment = new();
        experiment.TestSudokuWalkSize(sudoku);
    }

    private static bool GetProgramDefaults()
    {
        Console.Write("Show intermediate steps? Y : N ");
        char input = char.ToUpper(Console.ReadKey().KeyChar);

        // Enter recursion if the input was not a valid sudoku index.
        if (input != 'Y' && input != 'N')
        {
            Console.WriteLine("Invalid option input.", Console.ForegroundColor = ConsoleColor.Red);
            Console.ResetColor();
            return GetProgramDefaults();
        }

        return input == 'Y';
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

    private static string ReadInputFile(int? grid) {
        string[] lines = File.ReadAllLines("../../../sudoku_input.txt");

        // The input file will have the appropriate sudoku input on each second line.
        return lines[(int)grid! * 2 - 1];
    }
}
