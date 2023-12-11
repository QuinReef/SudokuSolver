namespace SudokuSolver;

public class SudokuSolver {
    public Sudoku? _currentPuzzle;
    private ushort[] _scores = new ushort[18];

    private ushort bestScore = 0;
    private ushort RandomRestartTokens = 0;
    private ushort RandomWalkTokens = 0;
    private ushort MaxIterations = 0;
    private double BiasedProbabilty = 1;


    public SudokuSolver(Sudoku puzzle, ushort _RandomRestartTokens, ushort _RandomWalkTokens, ushort _MaxIterations, double _BiasedProbabilty) {
        // Initialize currentGrid with the first Sudoku grid
        _currentPuzzle = puzzle;

        // Initialize algorithm params  
        RandomRestartTokens = _RandomRestartTokens;
        RandomWalkTokens = _RandomWalkTokens;
        MaxIterations = _MaxIterations;
        BiasedProbabilty = _BiasedProbabilty;

        bestScore = InitializeSudokuScore();
    }

    /// <summary>
    /// Gets the initial sudoku score 
    /// </summary>
    public ushort InitializeSudokuScore() {
        for (ushort i = 0; i < 9; i++) {
            _scores[i] = Evaluate(_currentPuzzle.GetRowValues(i));
            _scores[9 + i] = Evaluate(_currentPuzzle.GetColumnValues(i));
        }
        return GetHeuristicScore();
    }

    /// <summary>
    /// Evaluate the score for an array of values
    /// </summary>
    private ushort Evaluate(ushort[] values) {
        HashSet<ushort> uniqueValues = new HashSet<ushort>(values);

        return (ushort)(9 - uniqueValues.Count);
    }

    /// <summary>
    /// Adds all the _scores values
    /// </summary>
    /// <returns>Returns the heuristics Score for the sudoku board</returns>
    public ushort GetHeuristicScore() {
        ushort score = 0;
        for (int i = 0; i < _scores.Length; i++) {
            score += _scores[i];
        }
        return score;
    }

    /// <summary>
    /// Given a row and column update the scores list
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <returns>Updated sudoku game heuristic score</returns>
    public ushort updateHeuristicScore((ushort row, ushort column) coord1, (ushort row, ushort column) coord2) {
        _scores[coord1.row] = Evaluate(_currentPuzzle.GetRowValues(coord1.row));
        _scores[coord1.column + 9] = Evaluate(_currentPuzzle.GetRowValues(coord1.column));
        _scores[coord2.row] = Evaluate(_currentPuzzle.GetRowValues(coord2.row));
        _scores[coord2.column + 9] = Evaluate(_currentPuzzle.GetRowValues(coord2.column));

        return GetHeuristicScore();
    }


    /// <summary>
    /// Generates the succesors ordered by ascending heuristic value
    /// </summary>
    /// <returns></returns>
    private List<(Sudoku, int)> GetSuccessorsOrderedByScore() {
        List<(Sudoku, int)> successors = new List<(Sudoku, int)>();

        // Randomly select a cluster
        ushort clusterIndex = (ushort)new Random().Next(0, 9);

        SudokuCluster cluster = _currentPuzzle.GetClusters()[clusterIndex];

        HashSet<(ushort, ushort)> nonFixedPositions = cluster.RetrieveInvalidCells();

        for (int i = 0; i < nonFixedPositions.Count; i++) {
            for (int j = i + 1; j < nonFixedPositions.Count; j++) {
                var cell1 = nonFixedPositions.ElementAt(i);
                var cell2 = nonFixedPositions.ElementAt(j);

                cluster.SwapCells(cell1, cell2);

                //int tempScore = updateHeuristicScore(cell1, cell2); 
                //successors.Add((newPuzzle, tempScore));
            }
        }

        // Order successors by improved score (ascending order)
        successors = successors.OrderBy(successor => successor.Item2).ToList();

        return successors;
    }


}
