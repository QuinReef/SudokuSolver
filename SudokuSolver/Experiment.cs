namespace SudokuSolver;

public class Experiment
{
    //list of random walk sizes to test with
    private List<ushort> _randomWalkExperimentSizes =
        new() {9};
    //list of sample sizes to test with
    private List<ushort> _sampleSizes = new() {25};
    public Experiment()
    {
 
    }

    /// <summary>
    /// Function to test the effect of the random walk size on the time it takes to solve a sudoku.
    /// </summary>
    /// <param name="subject"></param>
    public void TestSudokuWalkSize(Sudoku subject)
    {
        using (StreamWriter writer = new StreamWriter($"../../../testSizeResults5.txt"))
        {
            foreach (var sampleSize in _sampleSizes) {
                    Console.WriteLine(sampleSize);
                    writer.WriteLine(sampleSize);
                foreach (var testSize in _randomWalkExperimentSizes) {
                    writer.Write($"{testSize}:");
                    for (int i = 0; i < sampleSize; i++) {
                        SudokuSolver solver = new((Sudoku)subject.Clone(), testSize, false);
                        long timeMili = solver.Start();
                        writer.Write($"{timeMili},");
                    }

                    writer.WriteLine();

                }
            }
        }



    }
}