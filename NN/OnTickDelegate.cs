namespace JBSnorro.NN;

public delegate void OnTickDelegate(IMachine sender, OnTickEventArgs e);

public sealed class OnTickEventArgs
{
    public int Time { get; internal init; }
    public int EmittingAxonCount { get; internal set; }
    /// <summary>
    /// The number of fired neurons.
    /// </summary>
    public int ActivationCount { get; internal set; }
    /// <summary>
    /// Gets the current charges of the output neurons. A reference to this should not be stored, as the underlying structure is reused.
    /// </summary>
    public required IReadOnlyList<float> Output { get; set; }

    public static void WriteTickDataToConsole(IMachine sender, OnTickEventArgs e)
    {
        Console.WriteLine($"t={e.Time:d2}, emits: {e.EmittingAxonCount}, acts: {e.ActivationCount}");
    }
}
