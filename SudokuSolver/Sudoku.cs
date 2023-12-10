namespace SudokuSolver;

/// <summary>
/// Represents a 3x3 grid on the sudoku board.
/// </summary>
public class SudokuCluster {
    // All values present within each cell in the cluster.
    private ushort[,] _cells = new ushort[3,3];
    // All values with incorrect values that still need to be changed.
    private HashSet<(ushort,ushort)> _invalidCells = new();

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
}
