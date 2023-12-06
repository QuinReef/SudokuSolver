using System;
using System.IO;
namespace Sudoku {
    class SudokuSolver
    {
        public List<int[,]> sudokuGrids = new();

        static Random random = new();

        public SudokuSolver(string path)
        {
            LoadSudokuFromFile(path);
            
        }

        //Read Sudoku Puzzle from a File
        void LoadSudokuFromFile(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);

                for (int k = 1; k < lines.Length; k += 2)
                {
                    string[] values = lines[k].Split(' ').Skip(1).ToArray();
                    int[,] sudokuFields = new int[9,9];
                    int valCounter = 0;
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            sudokuFields[i, j] = int.Parse(values[valCounter]);
                            valCounter++;
                        }
                    }
                    sudokuGrids.Add(sudokuFields);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading Sudoku puzzle from file: " + ex.Message);
            }
        }

        //Represent Sudoku Puzzle
        public void PrintGrid(int[,] grid)
        {
            Console.Write("\n------------------\n");

            for (int i = 0; i < 9; i++)
            {
                Console.Write("|");

                for (int j = 0; j < 9; j++)
                {   
                   

                    if (j % 3 == 2 )
                        Console.Write( grid[i, j] + "|" );
                    else
                        Console.Write( grid[i, j]+ " ");
                }

                if (i % 3 == 2)
                    Console.Write("\n------------------\n");
                else
                    Console.WriteLine();
            }
        }


    }
    class Program
    {
        static void Main(string[] args)
        {
            SudokuSolver sv = new("sudoku_input.txt");
            sv.PrintGrid(sv.sudokuGrids[0]);
        }
    }
}