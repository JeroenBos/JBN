namespace JBSnorro.NN;

/// <summary>
/// Passing-through data that axons will receive after a step for updating update themselves.
/// It is obtained from <see cref="INetworkFactory.GetFeedback"/>.
/// </summary>
public interface IFeedback
{
    /// <summary>
    /// Whether the network should abort its execution loop.
    /// </summary>
    public bool Stop { get; }
}

/// <inheritdoc cref="INetworkFactory.GetFeedback(ReadOnlySpan{float}, IReadOnlyClock)"/>
public delegate IFeedback? GetFeedbackDelegate(ReadOnlySpan<float> latestOutput, IReadOnlyClock clock);
