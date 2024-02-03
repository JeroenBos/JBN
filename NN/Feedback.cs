namespace JBSnorro.NN;

public interface IFeedback
{
    /// <summary>
    /// Whether the network should abort it's execution loop.
    /// </summary>
    public bool Stop { get; }
}

/// <inheritdoc cref="INetworkFactory.GetFeedback(ReadOnlySpan{float}, IReadOnlyClock)"/>
public delegate IFeedback? GetFeedbackDelegate(ReadOnlySpan<float> latestOutput, IReadOnlyClock clock);
