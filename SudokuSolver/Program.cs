using static SudokuSolver.SudokuSolver;

namespace SudokuSolver;

class SudokuSolver
{
    public List<SudokuGrid> sudokuGrids = new List<SudokuGrid>();

    static Random random = new Random();

    public SudokuGrid currentGrid;

    /// <summary>
    /// Represents the amount of rows and columns on a <see cref="SudokuGrid"/>.
    /// </summary>
    private const ushort SudokuSize = 9;

    public SudokuSolver(string path)
    {
        public List<SudokuGrid> sudokuGrids = new List<SudokuGrid>();

        static Random random = new Random();

        public SudokuGrid currentGrid;


        public SudokuSolver(string path)
        {
            LoadSudokuFromFile(path);
            FillMissingValues();
            Console.WriteLine("Score: " + EvaluateGrid(currentGrid));
        }

        //Tuple representing a cell in the Sudoku
        public record SudokuCell(ushort Value, bool IsStatic);

        // Named 2D Array representing the Sudoku grid
        public record SudokuGrid(SudokuCell[,] Cells);

        // Read Sudoku Puzzle from input file
        void LoadSudokuFromFile(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);

                // Only process the lines with data
                for (int k = 1; k < lines.Length; k += 2)
                {
                    // As the first input is " ", shift the input 1 to the left 
                    string[] values = lines[k].Split(' ').Skip(1).ToArray();

                    SudokuCell[,] sudokuCells = new SudokuCell[9, 9];

                    // Counter for item in the values list.
                    int valCounter = 0;
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            ushort itemVal = ushort.Parse(values[valCounter]);
                            bool isStatic = itemVal != 0;
                            sudokuCells[i, j] = isStatic ? new SudokuCell(itemVal, true) : new SudokuCell(0, false);

                            valCounter++;
                        }
                    }

                    // Add to class variable
                    sudokuGrids.Add(new SudokuGrid(sudokuCells));
                }

                // Initialize currentGrid with the first Sudoku grid
                currentGrid = sudokuGrids.First();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading Sudoku puzzle from file: " + ex.Message);
            }
        }

        // Fill missing values in each 3x3 grid with random numbers
        private void FillMissingValues()
        {
            for (int i = 0; i < 9; i += 3)
            {
                for (int j = 0; j < 9; j += 3)
                {
                    //TODO: Get this data while reading the input file
                    // Create a list of available numbers (1-9)
                    List<ushort> availableNumbers = Enumerable.Range(1, 9).Select(x => (ushort)x).ToList();

                    // Remove numbers already present in the current 3x3 grid
                    for (int row = i; row < i + 3; row++)
                    {
                        for (int col = j; col < j + 3; col++)
                        {
                            if (currentGrid.Cells[row, col].IsStatic)
                            {
                                availableNumbers.Remove(currentGrid.Cells[row, col].Value);
                            }
                        }
                    }

                    // Fill each cell in the 3x3 grid
                    for (int row = i; row < i + 3; row++)
                    {
                        for (int col = j; col < j + 3; col++)
                        {
                            if (!currentGrid.Cells[row, col].IsStatic)
                            {
                                // Choose a random number from the available numbers
                                int randomIndex = random.Next(availableNumbers.Count);
                                currentGrid.Cells[row, col] = new SudokuCell(availableNumbers[randomIndex], false);

                                // Remove the chosen number from the available numbers
                                availableNumbers.RemoveAt(randomIndex);
                            }
                        }
                    }
                }
            }
        }

        // Represent Sudoku Puzzle
        public static void PrintGrid(SudokuGrid grid)
        {
            // Top Line
            Console.Write("\n#-----------------#\n");

            // For each Row
            for (int i = 0; i < 9; i++)
            {
                Console.Write("|");

                // For each Column
                for (int j = 0; j < 9; j++)
                {
                    // Every third item print a vertical line
                    if (j % 3 == 2)
                        Console.Write(grid.Cells[i, j].Value + "|");
                    else
                        Console.Write(grid.Cells[i, j].Value + " ");
                }

                // Every third row print a divider 
                if (i % 3 == 2)
                    Console.Write("\n#-----------------#\n");
                else
                    Console.WriteLine();
            }
        }


        private int EvaluateGrid(SudokuGrid grid)
        {
            int score = 0;

            // Evaluate rows
            for (int i = 0; i < 9; i++)
            {
                HashSet<ushort> colNumbers = new HashSet<ushort>();
                HashSet<ushort> rowNumbers = new HashSet<ushort>();
                for (int j = 0; j < 9; j++)
                {
                    rowNumbers.Add(grid.Cells[i, j].Value);
                    colNumbers.Add(grid.Cells[j, i].Value);

                }
                score += 18 - rowNumbers.Count - colNumbers.Count; // Number of missing values in the row
            }

            return score;
        }

        private SudokuGrid SwapValues((int, int) coord1, (int, int) coord2)
        {
            SudokuGrid newGrid = currentGrid;
            SudokuCell temp = currentGrid.Cells[coord1.Item1, coord1.Item2];
            newGrid.Cells[coord1.Item1, coord1.Item2] = currentGrid.Cells[coord2.Item1, coord2.Item2];
            newGrid.Cells[coord2.Item1, coord2.Item2] = temp;
            return newGrid;
        }
        //argmax{h(s) | s in successors(t)};
        private (SudokuGrid, int) SwapValues(ushort blockRow, ushort blockCol, int bestScore)
        {
            // Get the indices of two random non-fixed cells within the same block
            List<(ushort, ushort)> nonFixedCells = GetNonFixedCellsInBlock(blockRow, blockCol);
            SudokuGrid bestGrid = new SudokuGrid((SudokuCell[,])currentGrid.Cells.Clone());
            if (nonFixedCells.Count < 2)
            {
                // There are not enough non-fixed cells to perform a swap
                return (currentGrid, bestScore);
            }

            //For each of the permutations of the free cells in this cluster (3x3) 
            for (int i = 0; i < nonFixedCells.Count; i++)
            {
                for (int j = i + 1; j < nonFixedCells.Count; j++)
                {
                    var cell1 = nonFixedCells[i];
                    var cell2 = nonFixedCells[j];

                    //Swap  cells 
                    SudokuGrid newGrid = new SudokuGrid((SudokuCell[,])currentGrid.Cells.Clone());
                    SudokuCell temp = (currentGrid.Cells[cell1.Item1, cell1.Item2]);
                    newGrid.Cells[cell1.Item1, cell1.Item2] = currentGrid.Cells[cell2.Item1, cell2.Item2];
                    newGrid.Cells[cell2.Item1, cell2.Item2] = temp;

                    int tempScore = EvaluateGrid(newGrid);
                    if (tempScore < bestScore)
                    {
                        Console.WriteLine("In Row:" + blockRow + " & Col:" + blockCol + " Swapped: " + newGrid.Cells[cell1.Item1, cell1.Item2].Value + " & " + newGrid.Cells[cell2.Item1, cell2.Item2].Value);
                        bestGrid = newGrid;
                        bestScore = tempScore;
                    }


                }
            }

            return (bestGrid, bestScore);
        }

        private List<(ushort, ushort)> GetNonFixedCellsInBlock(ushort blockRow, ushort blockCol)
        {
            List<(ushort, ushort)> nonFixedCells = new List<(ushort, ushort)>();

            for (int i = (blockRow * 3); i < (blockRow * 3) + 3; i++)
            {
                for (int j = (blockCol * 3); j < (blockCol * 3) + 3; j++)
                {
                    if (!currentGrid.Cells[i, j].IsStatic)
                    {
                        nonFixedCells.Add(((ushort)i, (ushort)j));
                    }
                }
            }

            return nonFixedCells;
        }

      

        public void HillClimbing()
        {
            int maxIterations = 1000000;
            int iterations = 0;
            int localMaximum = 0;
            int bestScore = EvaluateGrid(currentGrid);

            //while h(t’) ≥ h(t) do t ← t’; t’ ← argmax{ h(s) | s in successors(t)}
            while (iterations < maxIterations)
            {
                (SudokuGrid temp, int newScore) = SwapValues((ushort)random.Next(0, 3), (ushort)random.Next(0, 3), bestScore);

                //There is a succesor with a better score than the current 
                if (newScore < bestScore)
                {
                    bestScore = newScore;
                    Console.WriteLine(bestScore + " NEW BEST");
                    PrintGrid(temp);
                    currentGrid = temp;
                    localMaximum = 0;
                }
                else
                {
                    localMaximum++;
                }


                iterations++;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            SudokuSolver sv = new SudokuSolver("../../../sudoku_input.txt");
            SudokuSolver.PrintGrid(sv.currentGrid);
            sv.HillClimbing();
            SudokuSolver.PrintGrid(sv.currentGrid);
            Console.ReadLine();

        LoadSudokuFromFile(path);
        FillMissingValues();
        Console.WriteLine("Score: " + EvaluateGrid(currentGrid));
    }

    //Tuple representing a cell in the Sudoku
    public record SudokuCell(ushort Value, bool IsStatic);

    // Named 2D Array representing the Sudoku grid
    public record SudokuGrid(SudokuCell[,] Cells);

    // Read Sudoku Puzzle from input file
    void LoadSudokuFromFile(string filePath)
    {
        try
        {
            string[] lines = File.ReadAllLines(filePath);

            // Only process the lines with data
            for (int k = 1; k < lines.Length; k += 2)
            {
                // As the first input is " ", shift the input 1 to the left 
                string[] values = lines[k].Split(' ').Skip(1).ToArray();

                SudokuCell[,] sudokuCells = new SudokuCell[9, 9];

                // Counter for item in the values list.
                int valCounter = 0;
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        ushort itemVal = ushort.Parse(values[valCounter]);
                        bool isStatic = itemVal != 0;
                        sudokuCells[i, j] = isStatic ? new SudokuCell(itemVal, true) : new SudokuCell(0, false);

                        valCounter++;
                    }
                }

                // Add to class variable
                sudokuGrids.Add(new SudokuGrid(sudokuCells));
            }

            // Initialize currentGrid with the first Sudoku grid
            currentGrid = sudokuGrids.First();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error reading Sudoku puzzle from file: " + ex.Message);
        }
    }

    // Fill missing values in each 3x3 grid with random numbers
    private void FillMissingValues()
    {
        for (int i = 0; i < 9; i += 3)
        {
            for (int j = 0; j < 9; j += 3)
            {
                //TODO: Get this data while reading the input file
                // Create a list of available numbers (1-9)
                List<ushort> availableNumbers = Enumerable.Range(1, 9).Select(x => (ushort)x).ToList();

                // Remove numbers already present in the current 3x3 grid
                for (int row = i; row < i + 3; row++)
                {
                    for (int col = j; col < j + 3; col++)
                    {
                        if (currentGrid.Cells[row, col].IsStatic)
                        {
                            availableNumbers.Remove(currentGrid.Cells[row, col].Value);
                        }
                    }
                }

                // Fill each cell in the 3x3 grid
                for (int row = i; row < i + 3; row++)
                {
                    for (int col = j; col < j + 3; col++)
                    {
                        if (!currentGrid.Cells[row, col].IsStatic)
                        {
                            // Choose a random number from the available numbers
                            int randomIndex = random.Next(availableNumbers.Count);
                            currentGrid.Cells[row, col] = new SudokuCell(availableNumbers[randomIndex], false);

                            // Remove the chosen number from the available numbers
                            availableNumbers.RemoveAt(randomIndex);
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Presents the current state of the sudoku puzzle in a proper format on the console interface.
    /// </summary>
    public static void PrintGrid(SudokuGrid grid) {
        Console.WriteLine("\n┌───────┬───────┬───────┐");
        
        for (int i = 0; i < SudokuSize; i++) { // rows
            Console.Write("│ ");
            
            for (int j = 0; j < SudokuSize; j++) { // columns
                SudokuCell cell = grid.Cells[i, j];

                // Give each value the appropriate colour.
                Console.ForegroundColor = DetermineColor(cell);
                Console.Write(cell.Value);
                Console.ResetColor();

                // Print a column divider between each cluster, or add spacing.
                Console.Write(j % 3 == 2 ? " │ " : " ");
            }

            // Print a full divider between each row of 3x3 clusters.
            if (i % 3 == 2 && i != SudokuSize - 1) {
                Console.WriteLine("\n├───────┼───────┼───────┤");
            }
            else {
                Console.WriteLine();
            }
        }

        Console.WriteLine("└───────┴───────┴───────┘");
    }

    /// <summary>
    /// Determines the colour of a <see cref="SudokuCell"/> based on its properties.
    /// </summary>
    /// <returns>If the cell is static, returns <see cref="ConsoleColor.DarkGray"/>; otherwise, returns <see cref="ConsoleColor.White"/>.</returns>
    private static ConsoleColor DetermineColor(SudokuCell cell) {
        return cell.IsStatic ? ConsoleColor.DarkGray : ConsoleColor.White;
    }

    private int EvaluateGrid(SudokuGrid grid)
    {
        int score = 0;

        // Evaluate rows
        for (int i = 0; i < 9; i++)
        {
            HashSet<ushort> rowNumbers = new HashSet<ushort>();
            for (int j = 0; j < 9; j++)
            {
                if (grid.Cells[i, j].Value != 0)
                {
                    rowNumbers.Add(grid.Cells[i, j].Value);
                }
            }
            score += 9 - rowNumbers.Count; // Number of missing values in the row
        }

        // Evaluate columns
        for (int j = 0; j < 9; j++)
        {
            HashSet<ushort> colNumbers = new HashSet<ushort>();
            for (int i = 0; i < 9; i++)
            {
                if (grid.Cells[i, j].Value != 0)
                {
                    colNumbers.Add(grid.Cells[i, j].Value);
                }
            }
            score += 9 - colNumbers.Count; // Number of missing values in the column
        }

        return score;
    }

}

class Program {
    static void Main(string[] args) {
        SudokuSolver sv = new SudokuSolver("../../../sudoku_input.txt");
        PrintGrid(sv.currentGrid);
        // SudokuSolver.PrintGrid(sv.currentGrid);
    }
}
