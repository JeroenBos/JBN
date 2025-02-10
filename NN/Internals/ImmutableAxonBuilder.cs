namespace JBSnorro.NN.Internals;

/// <summary>
/// Represents a simple immutable axon builder.
/// </summary>
[DebuggerDisplay("Axon")]
internal sealed class ImmutableAxonBuilder : IAxonBuilder
{
    internal ImmutableAxonBuilder(int length, IReadOnlyList<float> initialWeights, object startNeuronLabel, object endNeuronLabel)
    {
        IAxonBuilder.AssertPreconditions(length, initialWeights);
        this.Length = length;
        this.InitialWeights = initialWeights;
        this.StartNeuronLabel = startNeuronLabel;
        this.EndNeuronLabel = endNeuronLabel ?? throw new ArgumentNullException(nameof(endNeuronLabel));
    }

    public int Length { get; }
    public IReadOnlyList<float> InitialWeights { get; }
    public object StartNeuronLabel { get; }
    public object EndNeuronLabel { get; }
}
