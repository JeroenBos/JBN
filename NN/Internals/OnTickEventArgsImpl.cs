namespace JBSnorro.NN.Internals;

/// <inheritdoc />
internal sealed class OnTickEventArgsImpl : OnTickEventArgs
{
    public int Time { get; internal init; }
    public int EmittingAxonCount { get; internal set; }
    public int ExcitationCount { get; internal set; }

    private IReadOnlyList<float>? output;
    public IReadOnlyList<float> Output
    {
        get => output ?? throw new InvalidOperationException("Should have been set already");
        internal set => this.output = value ?? throw new ArgumentNullException(nameof(value));
    }
}
