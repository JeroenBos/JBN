﻿namespace JBSnorro.NN.Internals;

/// <summary>
/// Represents an axon that does not update its weights when given feedback.
/// </summary>
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
    public void UpdateWeights(float[] currentWeights, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, IFeedback feedback)
    {
    }
}
