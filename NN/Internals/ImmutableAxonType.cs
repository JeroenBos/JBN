namespace JBSnorro.NN.Internals;

/// <summary>
/// Represents an axon that does not update its weights when given feedback.
/// </summary>
internal sealed class ImmutableAxonType : BaseAxonType, IAxonType
{
    internal ImmutableAxonType(int length, IReadOnlyList<float> initialWeight) : base(length, initialWeight) { }

    public override void UpdateWeights(float[] currentWeights, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, IFeedback feedback)
    {
    }
}
