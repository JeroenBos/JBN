namespace JBSnorro.NN.Internals;

/// <summary>
/// Represents an axon that does not update its weights when given feedback.
/// </summary>
[DebuggerDisplay("Axon")]
internal sealed class ImmutableAxonType : IAxonBuilder
{
    internal ImmutableAxonType(int length, IReadOnlyList<float> initialWeight, int startNeuronIndex, int endNeuronIndex)
    {
        IAxonBuilder.AssertPreconditions(length, initialWeight);
        this.Length = length;
        this.InitialWeights = initialWeight;
        this.StartNeuronIndex = startNeuronIndex;
        this.EndNeuronIndex = endNeuronIndex;
    }

    public int Length { get; }
    public IReadOnlyList<float> InitialWeights { get; }
    public int StartNeuronIndex { get; }
    public int EndNeuronIndex { get; }

    public void UpdateWeights(float[] currentWeights, int timeSinceLastExcitation, float averageTimeBetweenExcitations, int excitationCount, IFeedback feedback)
    {
    }
}
