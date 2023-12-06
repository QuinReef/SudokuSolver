using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Sudoku.SudokuSolver;

namespace Sudoku
{
    class SudokuSolver
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
            Console.Write("\n------------------\n");

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
                    Console.Write("\n------------------\n");
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

    class Program
    {
        static void Main(string[] args)
        {
            SudokuSolver sv = new SudokuSolver("../../../sudoku_input.txt");
            SudokuSolver.PrintGrid(sv.currentGrid);
            SudokuSolver.PrintGrid(sv.currentGrid);

        }
    }

}
