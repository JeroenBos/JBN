namespace JBSnorro.NN.Internals;

/// <summary>
/// A helper implementation of <see cref="IAxonType"/> with parameter checking in place.
/// </summary>
internal abstract class BaseAxonType : IAxonType
{
    public int Length { get; }
    public IReadOnlyList<float> InitialWeights { get; }


    protected BaseAxonType(int length, IReadOnlyList<float> initialWeights)
    {
        if (length <= 0 || length > MAX_AXON_LENGTH)
            throw new ArgumentOutOfRangeException(nameof(length));
        if (initialWeights is null)
            throw new ArgumentNullException(nameof(initialWeights));
        if (initialWeights.Count == 0)
            throw new ArgumentException("At least one weight must be provided", nameof(initialWeights));
        if (initialWeights.Any(float.IsNaN))
            throw new ArgumentException("NaN is not valid", nameof(initialWeights));

        this.Length = length;
        this.InitialWeights = initialWeights;
    }

    public abstract void UpdateWeights(float[] currentWeights, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, IFeedback feedback);
}
