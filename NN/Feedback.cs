namespace JBSnorro.NN;

public interface IFeedback
{
    /// <summary>
    /// Whether the network should abort it's execution loop.
    /// </summary>
    public bool Stop { get; }
}

/// <inheritdoc cref="INetworkFactory.GetFeedback(ReadOnlySpan{float}, IReadOnlyClock)"/>
public delegate TFeedback? GetFeedbackDelegate<out TFeedback>(ReadOnlySpan<float> latestOutput, IReadOnlyClock clock) where TFeedback : class, IFeedback;

// The same as GetFeedbackDelegate, but without the generic return type, which can always be cast back when visible through the public surface.
internal delegate IFeedback? InternalGetFeedbackDelegate(ReadOnlySpan<float> latestOutput, IReadOnlyClock clock);
