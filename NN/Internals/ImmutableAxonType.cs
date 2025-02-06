namespace JBSnorro.NN.Internals;

/// <summary>
/// Represents an axon that does not update its weights when given feedback.
/// </summary>
[DebuggerDisplay("Axon")]
internal sealed class ImmutableAxonType : IAxonBuilder
{
    internal ImmutableAxonType(int length, IReadOnlyList<float> initialWeight, object startNeuronLabel, object endNeuronLabel)
    {
        IAxonBuilder.AssertPreconditions(length, initialWeight);
        this.Length = length;
        this.InitialWeights = initialWeight;
        this.StartNeuronLabel = startNeuronLabel;
        this.EndNeuronLabel = endNeuronLabel;
    }

    public int Length { get; }
    public IReadOnlyList<float> InitialWeights { get; }
    public object StartNeuronLabel { get; }
    public object EndNeuronLabel { get; }

    public void UpdateWeights(float[] currentWeights, int timeSinceLastExcitation, float averageTimeBetweenExcitations, int excitationCount, IFeedback feedback)
    {
    }
}
