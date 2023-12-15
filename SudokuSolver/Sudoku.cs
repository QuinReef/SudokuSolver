﻿namespace SudokuSolver;

/// <summary>
/// Represents the full sudoku grid with nine different <see cref="SudokuCluster"/> instances on it.
/// </summary>
public class Sudoku : ICloneable {
    // The raw user input representing the starting state of the sudoku grid.
    private string _grid;

    // The nine different clusters of the sudoku grid.
    // The clusters are laid out as follows:
    //      0 1 2
    //      3 4 5
    //      6 7 8
    private SudokuCluster[] _clusters = new SudokuCluster[9];
    // The heuristic values of each row and column.
    private ushort[] _heuristicValues = new ushort[18];

    public Sudoku(string input) {
        _grid = input;
    }

    /// <summary>
    /// Retrieves the full sudoku grid, represented by nine <see cref="SudokuCluster"/> instances.
    /// </summary>
    public SudokuCluster[] GetSudokuGrid() => _clusters;
    /// <summary>
    /// Retrieves the 1-dimensional array of all eighteen heuristic scores of the sudoku.
    /// The array firstly contains the nine columns, then the nine rows.
    /// </summary>
    public ushort[] GetHeuristicValues() => _heuristicValues;

    /// <summary>
    /// Loads each value from the initial input string into the appropriate <see cref="SudokuCluster"/>.
    /// </summary>
    public void Load() {
        // Convert the input into a list of ushorts, skipping the initial white space.
        ushort[] values = _grid.Split(' ').Skip(1).Select(ushort.Parse).ToArray();

        // Input is read based on 3x1 clusters per iteration "i".
        for (ushort i = 0; i < 3; i++) {
            // Three clusters represent the 3x1 row of clusters.
            SudokuCluster cluster1 = new(), cluster2 = new(), cluster3 = new();

            for (ushort row = 0; row < 3; row++) { // row in the three respective clusters
                for (ushort column = 0; column < 9; column++) { // column of each value in a respective row
                    ushort value = values[i * 27 + row * 9 + column];

                    // Determine the appropriate cluster.
                    switch (column / 3) {
                        case 0:
                            FillCluster(cluster1, value, row, column);
                            break;
                        case 1:
                            FillCluster(cluster2, value, row, (ushort)(column - 3));
                            break;
                        case 2:
                            FillCluster(cluster3, value, row, (ushort)(column - 6));
                            break;
                    }
                }
            }

            /* Add the finalised cluster to the sudoko grid.
               Firstly, however, all "empty" (0) values in the read cluster are replaced by appropriate, missing values. */
            _clusters[i * 3] = cluster1.FillMissingValues();
            _clusters[i * 3 + 1] = cluster2.FillMissingValues();
            _clusters[i * 3 + 2] = cluster3.FillMissingValues();
        }
    }

    // Helper function to fill the respective cluster's values, reducing boilerplate code.
    private void FillCluster(SudokuCluster cluster, ushort value, ushort row, ushort column) {
        cluster.AddCell((column, row), value);

        if (value == 0) {
            cluster.AddInvalidCell((column, row));
        } else {
            cluster.RemoveAvailableDigit(value);
        }
    }

    /// <summary>
    /// Presents the current state of the sudoku puzzle in a proper format on the console interface.
    /// </summary>
    public void Show() {
        Console.WriteLine("\n┌───────┬───────┬───────┐");

        /* The grid is printed in the same way that it is loaded.
           The grid is firstly split up into three 3x1 clusters of sudoku clusters. */
        for (int i = 0; i < 3; i++) {
            // Each row (3) in this cluster of clusters is then iterated over.
            for (int row = 0; row < 3; row++) {
                Console.Write("│ ");

                // Each value is printed from left-to-right per row.
                for (int column = 0; column < 9; column++) {
                    Console.Write($"{_clusters[i * 3 + column / 3].RetrieveCells()[(ushort)(column % 3), row]} ");

                    if ((column + 1) % 3 == 0) {
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
    /// Returns all values in a given row from left to right.
    /// </summary>
    /// <param name="row">The 0-based row index.</param>
    public ushort[] GetRowValues(ushort row) {
        ushort[] values = new ushort[9];

        for (int y = 0; y < 9; y++) {
            values[y] = _clusters[row / 3 * 3 + y / 3].RetrieveCells()[(ushort)(y % 3), row % 3];
        }

        return values;
    }

    /// <summary>
    /// Returns all values in a given column from top to bottom.
    /// </summary>
    /// <param name="column">The 0-based column index.</param>
    public ushort[] GetColumnValues(ushort column) {
        ushort[] values = new ushort[9];

        for (int x = 0; x < 9; x++) {
            values[x] = _clusters[x / 3 * 3 + column / 3].RetrieveCells()[(ushort)(column % 3), x % 3];
        }

        return values;
    }

    // The implementation of Clone() from the ICloneable interface to enable deep-copying.
    public object Clone() {
        SudokuCluster[] newclusters = new SudokuCluster[9];
        for (int i = 0; i < _clusters.Length; i++) {
            newclusters[i] = (SudokuCluster)_clusters[i].Clone();
        }

        ushort[] scores = new ushort[18];
        Array.Copy(_heuristicValues, scores, _heuristicValues.Length);

        return new Sudoku(_grid) {
            _clusters = newclusters,
            _heuristicValues = scores
        };
    }
}

/// <summary>
/// Represents a 3x3 grid on a <see cref="Sudoku"/> board.
/// </summary>
public class SudokuCluster : ICloneable {
    // All values present within each cell in the cluster.
    private ushort[,] _cells = new ushort[3, 3];
    // All cells with incorrect values that still need to be changed.
    private HashSet<(ushort, ushort)> _invalidCells = new();
    /* All values 1-9 that are not yet present in the cluster.
       This list is solely used for the initialisation of the cluster at the start of the program. */
    private HashSet<ushort> _availableValues = Enumerable.Range(1, 9).Select(x => (ushort)x).ToHashSet();

    /// <summary>
    /// Retrieves all values present within each cell in the cluster.
    /// </summary>
    public ushort[,] RetrieveCells() => _cells;

    /// <summary>
    /// Retrieves the coordinates of all values with incorrect values that still need to be changed.
    /// </summary>
    public HashSet<(ushort, ushort)> RetrieveInvalidCells() => _invalidCells;

    /// <summary>
    /// Adds a cell to the cluster.
    /// </summary>
    /// <param name="coord">The coordinates in the 2d cells array at which to insert the value.</param>
    /// <param name="value">The value that should be inserted in the cell.</param>
    public void AddCell((ushort, ushort) coord, ushort value) {
        _cells[coord.Item1, coord.Item2] = value;
    }

    /// <summary>
    /// Adds the coordinates of a cell to the set of invalid cells.
    /// </summary>
    public void AddInvalidCell((ushort, ushort) coord) {
        _invalidCells.Add(coord);
    }

    /// <summary>
    /// Adds a value to the set of available values in the cluster.
    /// </summary>
    public void RemoveAvailableDigit(ushort value) {
        _availableValues.Remove(value);
    }

    /// <summary>
    /// Swaps the values of two cells in the cluster.
    /// The function is its own inverse.
    /// </summary>
    public void SwapCells((ushort, ushort) coord1, (ushort, ushort) coord2) {
        (_cells[coord1.Item1, coord1.Item2], _cells[coord2.Item1, coord2.Item2]) =
            (_cells[coord2.Item1, coord2.Item2], _cells[coord1.Item1, coord1.Item2]);
    }

    /// <summary>
    /// Fills missing values in a <see cref="SudokuCluster"/> with random, remaining values.
    /// </summary>
    public SudokuCluster FillMissingValues() {
        Random random = new();

        // For each empty position in a cluster, randomly generate a valid value.
        foreach ((ushort x, ushort y) in _invalidCells) {
            int index = random.Next(_availableValues.Count);
            ushort num = _availableValues.ElementAt(index);
            _cells[x, y] = num;
            _availableValues.Remove(num);
        }

        return this;
    }

    // The implementation of Clone() from the ICloneable interface to enable deep-copying.
    public object Clone() {
        SudokuCluster clonedCluster = new() {
            _cells = new ushort[3, 3],
            _invalidCells = new HashSet<(ushort, ushort)>(_invalidCells),
            _availableValues = _availableValues
        };

        Array.Copy(_cells, clonedCluster._cells, _cells.Length);

        return clonedCluster;
    }
}
