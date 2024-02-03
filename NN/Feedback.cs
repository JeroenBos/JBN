namespace JBSnorro.NN;

public sealed class Feedback
{
    public float Dopamine { get; init; }
    public float Cortisol { get; init; }
    /// <summary>
    /// Whether the network should abort it's execution loop.
    /// </summary>
    public bool Stop { get; init; }
}

/// <inheritdoc cref="INetworkFactory.GetFeedback(ReadOnlySpan{float}, IReadOnlyClock)"/>
public delegate Feedback? GetFeedbackDelegate(ReadOnlySpan<float> latestOutput, IReadOnlyClock clock);
