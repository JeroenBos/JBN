namespace JBSnorro.NN;

public interface IAxonInitialization
{
    /// <summary>
    /// An argument passed to <see cref="GetAxonConnectionDelegate"/> indicating the neuron connected to is an (imagined) input neuron.
    /// </summary>
    public const int FROM_INPUT = -1;

    public int Length { get; }
    internal static void AssertPreconditions(int length, IReadOnlyList<float> initialWeights)
    {
        if (length <= 0 || length > MAX_AXON_LENGTH)
            throw new ArgumentOutOfRangeException(nameof(length));
        if (initialWeights is null)
            throw new ArgumentNullException(nameof(initialWeights));
        if (initialWeights.Count == 0)
            throw new ArgumentException("At least one weight must be provided", nameof(initialWeights));
        if (initialWeights.Any(float.IsNaN))
            throw new ArgumentException("NaN is not valid", nameof(initialWeights));
    }
    public static IAxonInitialization Create(int length)
    {

    }
}

/// <param name="neuronFromIndex"> <see cref="IAxonInitialization.FROM_INPUT"/> can mean from input</param>
/// <returns> Returns the length of the axon; or null if not axon exists between the two specified neuron indices. </returns>
public delegate IAxonInitialization? GetAxonConnectionDelegate(int neuronFromIndex, int neuronToIndex);
/// <param name="currentWeights">
/// The weights with which a new axon of this type is to be initialized. One per charge.
/// Note that charge is something else than chemicals:
/// a neuron has different charges and an axon (upon emission) delivers those charges (weighted);
/// the chemicals are the fields that are extended by the neurons (and axons?) to determine growth.
/// So charges represent a bit neurotransmitters.
/// </param>
public delegate void UpdateWeightsDelegate(float[] currentWeights, int timeSinceLastExcitation, float averageTimeBetweenExcitations, int excitationCount, IFeedback feedback);
