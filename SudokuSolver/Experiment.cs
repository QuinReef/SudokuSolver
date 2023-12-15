namespace SudokuSolver;

public class Experiment
{
    private List<ushort> _randomWalkExperimentSizes =
        new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 20, 25, 30, 40,50,100,200 };
    private List<ushort> _sampleSizes = new() {5};
    public Experiment()
    {
 
    }

    public void TestSudokuWalkSize(Sudoku subject)
    {
        using (StreamWriter writer = new StreamWriter($"../../../testSizeResults.txt"))
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