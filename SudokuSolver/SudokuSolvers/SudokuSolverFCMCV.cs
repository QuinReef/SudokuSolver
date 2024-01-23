namespace SudokuSolver.SudokuSolvers;

public class SudokuSolverFCMCV : SudokuSolverFC
{
    public SudokuSolverFCMCV(Sudoku sudoku, bool showSteps) : base(sudoku, showSteps) { }

    public override void Solve()
    {
        /* Firstly, make the initial Sudoku node consistent by updating domains based on fixed values,
           then start solving using Forward Checking. */
        MakeNodeConsistent();
        SolveSudoku();
    }

    protected Tuple<Cell, (ushort, ushort)>? FindSmallestEmptyCellByDomainSize()
    {
        Tuple<Cell, (ushort, ushort)> smallestEmptyCell = null;

        for (ushort row = 0; row < 9; row++)
        {
            for (ushort column = 0; column < 9; column++)
            {
                Cell cell = ActiveSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3].RetrieveCells()[column % 3, row % 3];

                if (cell.Value == 0)
                {
                    int domainSize = cell.Domain?.Count ?? 0;

                    if (smallestEmptyCell == null || domainSize < smallestEmptyCell.Item1.Domain?.Count)
                    {
                        smallestEmptyCell = new Tuple<Cell, (ushort, ushort)>(cell, (row, column));
                    }
                }
            }
        }

        return smallestEmptyCell;
    }

    private bool SolveSudoku()
    {
        Tuple<Cell, (ushort, ushort)> emptyCell = FindSmallestEmptyCellByDomainSize();

        // If no empty cells remain, the puzzle is solved.
        if (emptyCell == null)
        {
            ActiveSudoku.Show();
            return true;
        }

        // Print relevant information to the console.
        Print();


        (Cell emptyCellObject, (ushort row, ushort column)) = emptyCell;

        foreach (ushort value in new HashSet<ushort>(emptyCellObject.Domain!))
        {
            if (IsValidMove(ActiveSudoku, row, column, value))
            {
                SudokuCluster cluster = ActiveSudoku.GetSudokuGrid()[row / 3 * 3 + column / 3];

                PerformMove((column, row), cluster, new Cell(value, emptyCellObject.IsFixed, new HashSet<ushort>()));

                UpdateDomains((column, row), cluster, value, false);
                if (SolveSudoku())
                {
                    return true;
                }

                PerformMove((column, row), cluster, emptyCellObject);
                UpdateDomains((column, row), cluster, value, true);
            }
        }

        return false;
    }
}
