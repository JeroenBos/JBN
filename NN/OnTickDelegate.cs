using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public delegate void OnTickDelegate(IMachine sender, OnTickEventArgs e);


public sealed class OnTickEventArgs : OnFeedEventArgs
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
    internal OnTickEventArgs(int time, IReadOnlyList<Axon> inputAxons)
    {
        this.Time = time;
        this._inputAxons = inputAxons ?? throw new ArgumentNullException(nameof(inputAxons));
    }

    /// <summary>
    /// Gets or sets whether the network should abort its execution loop.
    /// </summary>
    public bool Stop { get; set; }

    /// <summary>
    /// Gets or sets feedback that the network will use to update its internal state. This feedback is passed opaquely to <see cref="IAxonType.UpdateWeights"/> 
    /// </summary>
    public IFeedback? Feedback { get; set; }

    private readonly IReadOnlyList<Axon> _inputAxons;
    IReadOnlyList<Axon> OnFeedEventArgs.inputAxons => _inputAxons;
}

public interface OnFeedEventArgs
{
    /// <inheritdoc cref="OnTickEventArgs.Time"/>
    int Time { get; }
    /// <summary>
    /// Gets the input axons on the network. 
    /// Preferably an internal implementation for a refactor, just to get tests to pass.
    /// Network feeders should have a better way to access the input axons.
    /// </summary>
    internal IReadOnlyList<Axon> inputAxons { get; }
}
