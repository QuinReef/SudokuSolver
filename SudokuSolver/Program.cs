namespace SudokuSolver;

public class SudokuSolver {
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
    private void LoadSudokuFromFile(string filePath) {
        try {
            string[] lines = File.ReadAllLines(filePath);

            // Only process the lines with data
            for (int k = 1; k < lines.Length; k += 2) {
                // Convert the input into a list of ushorts, skipping the initial white space.
                ushort[] values = lines[k].Split(' ').Skip(1).Select(ushort.Parse).ToArray();

                SudokuCell[,] sudokuCells = new SudokuCell[9, 9];

                // Initialise the actual sudoku grid.
                ushort counter = 0;
                for (ushort i = 0; i < 9; i++) { // rows
                    for (ushort j = 0; j < 9; j++) { // column
                        ushort value = values[counter];
                        bool isStatic = value != 0;
                        sudokuCells[i, j] = new SudokuCell(value, isStatic);

                        counter++;
                    }
                }

                sudokuGrids.Add(new SudokuGrid(sudokuCells));
            }

            // Initialize currentGrid with the first Sudoku grid
            currentGrid = sudokuGrids.First();
        } catch (Exception e) {
            Console.WriteLine("Error reading Sudoku puzzle from file: " + e.Message);
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



    // public void HillClimbing()
    // {
    //     int maxIterations = 1000000;
    //     int iterations = 0;
    //     int localMaximum = 0;
    //     int bestScore = EvaluateGrid(currentGrid);
    //
    //     //while h(t’) ≥ h(t) do t ← t’; t’ ← argmax{ h(s) | s in successors(t)}
    //     while (iterations < maxIterations)
    //     {
    //         (SudokuGrid temp, int newScore) = SwapValues((ushort)random.Next(0, 3), (ushort)random.Next(0, 3), bestScore);
    //
    //         //There is a succesor with a better score than the current 
    //         if (newScore < bestScore)
    //         {
    //             bestScore = newScore;
    //             Console.WriteLine(bestScore + " NEW BEST");
    //             PrintGrid(temp);
    //             currentGrid = temp;
    //             localMaximum = 0;
    //         }
    //         else
    //         {
    //             localMaximum++;
    //         }
    //
    //
    //         iterations++;
    //     }
    // }
}
