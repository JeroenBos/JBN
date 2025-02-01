namespace JBSnorro.NN;

public delegate void OnTickDelegate(IMachine sender, OnTickEventArgs e);


public sealed class OnTickEventArgs : EventArgs
{
    /// <summary>
    /// The current time on the Machine's clock.
    /// </summary>
    public int Time { get; }
    /// <summary>
    /// The number of axons emitted.
    /// </summary>
    public int EmittingAxonCount { get; internal set; }
    /// <summary>
    /// The number of fired neurons.
    /// </summary>
    public int ExcitationCount { get; internal set; }
    /// <summary>
    /// Gets the current charges of the output neurons. A reference to this should not be stored, as the underlying structure is reused.
    /// </summary>
    public IReadOnlyList<float> Output
    {
        get => output ?? throw new InvalidOperationException("Should have been set already");
        internal set => this.output = value ?? throw new ArgumentNullException(nameof(value));
    }

    // must be set before we pass this class through public surface
    private IReadOnlyList<float>? output;
    [DebuggerHidden]
    internal OnTickEventArgs(int time, int inputAxonCount)
    {
        this.Time = time;
        this.InputAxonCount = inputAxonCount;
    }

    /// <summary>
    /// Gets or sets whether the network should abort its execution loop.
    /// </summary>
    public bool Stop { get; set; }

    /// <summary>
    /// Gets or sets feedback that the network will use to update its internal state. This feedback is passed opaquely to <see cref="IAxonType.UpdateWeights"/> 
    /// </summary>
    public IFeedback? Feedback { get; set; }

    /// <summary>
    /// Gets the number of input axons in the current network.
    /// </summary>
    public int InputAxonCount { get; }
}

