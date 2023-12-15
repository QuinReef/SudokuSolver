namespace SudokuSolver;

public class Experiment
{
    private List<ushort> _randomWalkExperimentSizes =
        new() {9};
    private List<ushort> _sampleSizes = new() {25};
    public Experiment()
    {
 
    }

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
                        long timeMili = solver.HillClimbing();
                        writer.Write($"{timeMili},");
                    }

                    writer.WriteLine();

                }
            }
        }



    }
}