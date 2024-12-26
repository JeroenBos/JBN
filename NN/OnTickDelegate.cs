namespace JBSnorro.NN;

public delegate void OnTickDelegate(IMachine sender, OnTickEventArgs e);


public interface OnTickEventArgs
{
    /// <summary>
    /// The current time on the Machine's clock.
    /// </summary>
    public int Time { get; }
    /// <summary>
    /// The number of axons emitted.
    /// </summary>
    public int EmittingAxonCount { get; }
    /// <summary>
    /// The number of fired neurons.
    /// </summary>
    public int ExcitationCount { get; }
    /// <summary>
    /// Gets the current charges of the output neurons. A reference to this should not be stored, as the underlying structure is reused.
    /// </summary>
    public IReadOnlyList<float> Output { get; }
}
