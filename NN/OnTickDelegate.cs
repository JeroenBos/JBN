namespace JBSnorro.NN;

public delegate void OnTickDelegate(IMachine sender, OnTickEventArgs e);

public sealed class OnTickEventArgs
{
    public int Time { get; internal init; }
    public int EmittingAxonCount { get; internal set; }
    public float PositiveCumulativeOomph { get; internal set; }
    public float NegativeCumulativeOomph { get; internal set; }
    public int ActivationCount { get; internal set; }

    /// <summary>
    /// 
    /// </summary>
    public required IReadOnlyList<float> Output { get; set; }

    public static void WriteTickDataToConsole(IMachine sender, OnTickEventArgs e)
    {
        Console.WriteLine($"t={e.Time:d2}, emits: {e.EmittingAxonCount}(Σ={e.PositiveCumulativeOomph:n2}/-{e.NegativeCumulativeOomph:n2}), acts: {e.ActivationCount}");
    }
}

