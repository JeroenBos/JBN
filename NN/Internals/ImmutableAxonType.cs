namespace JBSnorro.NN.Internals;

/// <summary>
/// Represents an axon that does not update its weights when given feedback.
/// </summary>
[DebuggerDisplay("Axon")]
internal sealed class ImmutableAxonType : IAxonType
{
    internal ImmutableAxonType(int length, IReadOnlyList<float> initialWeight)
    {
        IAxonType.AssertPreconditions(length, initialWeight);
        this.Length = length;
        this.InitialWeights = initialWeight;
    }

    public int Length { get; }
    public IReadOnlyList<float> InitialWeights { get; }
    public void UpdateWeights(float[] currentWeights, int timeSinceLastExcitation, float averageTimeBetweenExcitations, int excitationCount, IFeedback feedback)
    {
    }
}
