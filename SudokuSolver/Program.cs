using System;
using System.IO;
namespace Sudoku
{
    class SudokuSolver
    {
        public List<int[,]> sudokuGrids = new();

        static Random random = new();

        int[,] currentGrid = new int[9,9];

        public SudokuSolver(string path)
        {
            LoadSudokuFromFile(path);
            currentGrid = sudokuGrids[0];
            Console.WriteLine("Score:" + EvaluateRows());
        }

        //Read Sudoku Puzzle from a File
        void LoadSudokuFromFile(string filePath)
        {
            try
            {
                //Get entire input
                string[] lines = File.ReadAllLines(filePath);

                //Only process the lines with data
                for (int k = 1; k < lines.Length; k += 2)
                {
                    //As the first input is " ", shift the input 1 to the left 
                    string[] values = lines[k].Split(' ').Skip(1).ToArray();

                    int[,] sudokuFields = new int[9, 9];

                    //Counter for item in the values list.
                    int valCounter = 0;
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            sudokuFields[i, j] = int.Parse(values[valCounter]);
                            valCounter++;
                        }
                    }
                    //Add to class variable
                    sudokuGrids.Add(sudokuFields);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading Sudoku puzzle from file: " + ex.Message);
            }
        }

        //Represent Sudoku Puzzle
        public static void PrintGrid(int[,] grid)
        {
            //Top Line
            Console.Write("\n------------------\n");

            //For each Row
            for (int i = 0; i < 9; i++)
            {
                Console.Write("|");

                //For each Collumn
                for (int j = 0; j < 9; j++)
                {
                    //Every third item print a vertical line
                    if (j % 3 == 2)
                        Console.Write(grid[i, j] + "|");
                    else
                        Console.Write(grid[i, j] + " ");
                }

                //Every third row print an divider 
                if (i % 3 == 2)
                    Console.Write("\n------------------\n");
                else
                    Console.WriteLine();
            }
        }

        private int EvaluateRows()
        {
            int score = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if(currentGrid[i, j] == 0)
                        score++;
                }
            }
            return score;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            SudokuSolver sv = new("../../../sudoku_input.txt");
            SudokuSolver.PrintGrid(sv.sudokuGrids[0]);
        }
    }
}