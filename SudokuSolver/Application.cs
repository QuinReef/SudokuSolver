namespace SudokuSolver;

public class Application {
    public static void Main(string[] args) {
        // Retrieve the sudoku to solve from the user.
        int? grid = SelectGrid();

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
