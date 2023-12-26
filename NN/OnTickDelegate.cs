namespace JBSnorro.NN;

public delegate void OnTickDelegate(IMachine sender, OnTickEventArgs e);
public record OnTickEventArgs(
    int Time,
    int EmittingAxonCount,
    float PositiveCumulativeOomph,
    float NegativeCumulativeOomph,
    int ActivationCount
)
{
    public static void WriteTickDataToConsole(IMachine sender, OnTickEventArgs e)
    {
        Console.WriteLine($"t={e.Time:d2}, emits: {e.EmittingAxonCount}(Σ={e.PositiveCumulativeOomph:n2}/-{e.NegativeCumulativeOomph:n2}), acts: {e.ActivationCount}");
    }
}

