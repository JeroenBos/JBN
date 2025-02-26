using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

/// <summary>
/// "Builder" in the sense that this contains data from which an axon can be built.
/// </summary>
public interface IAxonBuilder
{
    /// <summary>
    /// An argument passed to <see cref="GetAxonConnectionDelegate"/> indicating the neuron connected to is an (imagined) input neuron.
    /// </summary>
    public const int FROM_INPUT = -1;
    /// <summary>
    /// Sentinel object for reference comparison. If an axon start has this as a neuron label, it's not a neuron but an input axon. 
    /// </summary>
    public static object FromInputLabel => InputSingleton.Instance;
    /// <summary>
    /// Creates an unchanging axon: one that does not update its weights.
    /// </summary>
    public static IAxonBuilder CreateImmutable(int length, IReadOnlyList<float> initialWeight, object startNeuronLabel, object endNeuronLabel)
    {
        return new ImmutableAxonBuilder(length, initialWeight, startNeuronLabel, endNeuronLabel);
    }

    /// <summary>
    /// The label identifying the start neuron of this axon.
    /// The default object equality comparison is used, that is <see cref="object.Equals(object?)"/> 
    /// </summary>
    public object StartNeuronLabel { get; }
    /// <summary>
    /// The label identifying the end neuron of this axon.
    /// The default object equality comparison is used, that is <see cref="object.Equals(object?)"/> 
    /// </summary>
    public object EndNeuronLabel { get; }
    public int Length { get; }
    /// <summary>
    /// The weights with which a new axon of this type is to be initialized. One weight per charge.
    /// Note that charge is something else than chemicals:
    /// a neuron has different charges and an axon (upon emission) delivers those charges (weighted);
    /// the chemicals are the fields that are extended by the neurons (and axons?) to determine growth.
    /// So charges represent a bit neurotransmitters.
    /// </summary>
    public IReadOnlyList<float> InitialWeights { get; }

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
}

public delegate IAxonBuilder? GetAxonConnectionDelegate(int neuronFromIndex, int neuronToIndex);

/// <param name="currentWeights">
/// The weights with which a new axon of this type is to be initialized. One per charge.
/// Note that charge is something else than chemicals:
/// a neuron has different charges and an axon (upon emission) delivers those charges (weighted);
/// the chemicals are the fields that are extended by the neurons (and axons?) to determine growth.
/// So charges represent a bit neurotransmitters.
/// </param>
public delegate void UpdateWeightsDelegate(float[] currentWeights, int timeSinceLastExcitation, float averageTimeBetweenExcitations, int excitationCount, IFeedback feedback, object? startLabel, object? endLabel);

public static class UpdateWeightsDelegateExtensions
{
    public static UpdateWeightsDelegate Empty { get; } = empty;
    private static void empty(float[] currentWeights, int timeSinceLastExcitation, float averageTimeBetweenExcitations, int excitationCount, IFeedback feedback, object? startLabel, object? endLabel)
    {
    }
}
public delegate void GetAxonLength(object? startLabel, object? endLabel);
