namespace SudokuSolver;

/// <summary>
/// Represents the full sudoku grid with nine different <see cref="SudokuCluster"/> instances on it.
/// </summary>
public class Sudoku : ICloneable {
    // The nine different clusters of the sudoku grid.
    // The clusters are laid out as follows:
    //      0 1 2
    //      3 4 5
    //      6 7 8
    private SudokuCluster[] _clusters = new SudokuCluster[9];
    // The raw user input representing the starting state of the sudoku grid.
    private string _grid;

    public Sudoku(string input) {
        _grid = input;
    }

    /// <summary>
    /// Retrieves the full sudoku grid, represented by nine <see cref="SudokuCluster"/> instances.
    /// </summary>
    public SudokuCluster[] GetClusters() => _clusters;

    /// <summary>
    /// Loads each value in the initial input string into the appropriate <see cref="SudokuCluster"/>.
    /// </summary>
    public void Load() {
        // Convert the input into a list of ushorts, skipping the initial white space.
        ushort[] values = _grid.Split(' ').Skip(1).Select(ushort.Parse).ToArray(); // O(n)

        // Input is read based on 3x1 clusters per iteration "i".
        for (ushort i = 0; i < 3; i++) {
            SudokuCluster cluster1 = new(), cluster2 = new(), cluster3 = new();

            for (ushort row = 0; row < 3; row++) { // row in the three respective clusters
                for (ushort j = 0; j < 9; j++) { // x-value of each value in a respective row
                    ushort value = values[i * 27 + row * 9 + j];

                    // Determine the appropriate cluster.
                    switch (j / 3) {
                        case 0:
                            cluster1.AddCell((j, row), value);

                            if (value == 0) {
                                cluster1.AddInvalidCell((j, row));
                            } else {
                                cluster1.AddFixedPosition((j, row));
                                cluster1.RemoveAvailableDigit(value);
                            }

                            break;
                        case 1:
                            cluster2.AddCell(((ushort)(j - 3), row), value);

                            if (value == 0) {
                                cluster2.AddInvalidCell(((ushort)(j - 3), row));
                            } else {
                                cluster2.AddFixedPosition(((ushort)(j - 3), row));
                                cluster2.RemoveAvailableDigit(value);
                            }
                            break;
                        case 2:
                            cluster3.AddCell(((ushort)(j - 6), row), value);

                            if (value == 0) {
                                cluster3.AddInvalidCell(((ushort)(j - 6), row));
                            } else {
                                cluster3.AddFixedPosition(((ushort)(j - 6), row));
                                cluster3.RemoveAvailableDigit(value);

                            }

                            break;
                    }
                }
            }

            _clusters[i * 3] = cluster1;
            _clusters[i * 3 + 1] = cluster2;
            _clusters[i * 3 + 2] = cluster3;
        }
    }

    /// <summary>
    /// Presents the current state of the sudoku puzzle in a proper format on the console interface.
    /// </summary>
    public void Print() {
        Console.WriteLine("\n┌───────┬───────┬───────┐");

        // The grid is printed in the same that is it loaded.
        // The grid is firstly split up into three 3x1 clusters of sudoku clusters.
        for (int i = 0; i < 3; i++) {
            // Each row (3) in this cluster of clusters is then iterated over.
            for (int row = 0; row < 3; row++) {
                Console.Write("│ ");

                // Each value is printed from left-to-right per row.
                for (int j = 0; j < 9; j++) {
                    Console.Write($"{_clusters[i * 3 + j / 3].RetrieveCells()[(ushort)(j % 3), row]} ");

                    if ((j + 1) % 3 == 0) {
                        Console.Write("│ ");
                    }
                }

                Console.WriteLine();
            }

            // Divider should be printed after each cluster of clusters, barring the bottom line.
            if (i < 2) {
                Console.WriteLine("├───────┼───────┼───────┤");
            }
        }

        Console.WriteLine("└───────┴───────┴───────┘");
    }

    /// <summary>
    /// Fills the Sudoku puzzle randomly.
    /// </summary>
    public void FillAllMissingValues() {
        // Fill each grid in the 3x3 puzzle
        for (int i = 0; i < 9; i++) {
            _clusters[i] = _clusters[i].FillMissingValues();
        }
    }

    /// <summary>
    /// Given a row index, return the suduko values corresponding with the column
    /// </summary>
    public ushort[] GetRowValues(ushort row) {
        ushort[] rowValues = new ushort[9];

        for (int j = 0; j < 9; j++) {
            rowValues[j] = _clusters[row / 3 * 3 + j / 3].RetrieveCells()[(ushort)(j % 3), row % 3];
        }

        return rowValues;
    }

    /// <summary>
    /// Given a column index, return the suduko values corresponding with the column
    /// </summary>
    public ushort[] GetColumnValues(ushort column) {
        ushort[] columnValues = new ushort[9];

        for (int i = 0; i < 3; i++) {
            columnValues[i * 3] = _clusters[i * 3 + column / 3 ].RetrieveCells()[(ushort)(column % 3), 0];
            columnValues[i * 3 + 1] = _clusters[i * 3 + column / 3 ].RetrieveCells()[(ushort)(column % 3), 1];
            columnValues[i * 3 + 2] = _clusters[i * 3 + column / 3 ].RetrieveCells()[(ushort)(column % 3), 2];
        }

        return columnValues;
    }

    public object Clone() {
        return new Sudoku(_grid) {
            _clusters = this._clusters
        };
    }
}

/// <summary>
/// Represents a 3x3 grid on the sudoku board.
/// </summary>
public class SudokuCluster : ICloneable {
    // All values present within each cell in the cluster.
    private ushort[,] _cells = new ushort[3,3];
    // All values with incorrect values that still need to be changed.
    private HashSet<(ushort,ushort)> _invalidCells = new();

    private HashSet<ushort> _avaibleDigits = Enumerable.Range(1, 9).Select(x => (ushort)x).ToHashSet();
    private HashSet<(ushort, ushort)> _fixedPosisitons = new();
    private readonly Random random = new Random();
    /// <summary>
    /// Retrieves all values present within each cell in the cluster.
    /// </summary>
    public ushort[,] RetrieveCells() => _cells;


    /// <summary>
    /// Retrieves the coordinates of all values with incorrect values that still need to be changed.
    /// </summary>
    public HashSet<(ushort, ushort)> RetrieveInvalidCells() => _invalidCells;

    /// <summary>
    /// Adds a cell to the cluster with the specified coordinates and value.
    /// </summary>
    public void AddCell((ushort,ushort) coords, ushort val) {
        _cells[coords.Item1,coords.Item2] = val; // O(1)
    }
    /// <summary>
    /// Adds a cell to the set of invalid cells with the specified coordinates.
    /// </summary>
    public void AddInvalidCell((ushort,ushort) coord) {
        _invalidCells.Add(coord); // O(1) -> add on hash set
    }
    /// <summary>
    /// Adds a cell to the set of non fixed cells with the specified coordinates.
    /// </summary>
    public void AddFixedPosition((ushort,ushort) coord)
    {
        _fixedPosisitons.Add(coord);
    }
    /// <summary>
    /// Adds a value to the set of free cluster digits
    /// </summary>
    public void RemoveAvailableDigit(ushort value)
    {
        _avaibleDigits.Remove(value);
    }
    /// <summary>
    /// Swaps the values of two cells in the cluster.
    /// </summary>
    public void SwapCells((ushort, ushort) coord1, (ushort, ushort) coord2) {
        (_cells[coord1.Item1, coord1.Item2], _cells[coord2.Item1, coord2.Item2]) =
            (_cells[coord2.Item1, coord2.Item2], _cells[coord1.Item1, coord1.Item2]);
    } // O(1) -> no search

    /// <summary>
    /// Removes a cell from the set of invalid cells when it has been deemed correct.
    /// </summary>
    public void ValidateCell((ushort, ushort) index) {
        _invalidCells.Remove(index); // O(1) -> remove on hash set
    }

    /// <summary>
    /// Fill missing values in a 3x3 grid with random numbers
    /// </summary>
    public SudokuCluster FillMissingValues()
    {
        HashSet<ushort> tempAvaibleNumbers = new HashSet<ushort>(_avaibleDigits);
        // Fill each avaible cell in the 3x3 grid

        foreach ((ushort x, ushort y) in _invalidCells)
        {
            int randomIndex = random.Next(tempAvaibleNumbers.Count);
            ushort randomNum = tempAvaibleNumbers.ElementAt(randomIndex);
            _cells[x, y] = randomNum;
            tempAvaibleNumbers.Remove(randomNum);
        }
        return this;
    }

    public object Clone() {
        return new SudokuCluster() {
            _cells = this._cells,
            _avaibleDigits = this._avaibleDigits,
            _fixedPosisitons = this._fixedPosisitons,
            _invalidCells = this._invalidCells
        };
    }
}
