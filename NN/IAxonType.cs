using System.Diagnostics;
using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface IAxonType
{
    /// <summary>
    /// An argument passed to <see cref="INetworkFactory.GetAxonConnection(int, int)"/> indicating the neuron connected to is an (imagined) input neuron.
    /// </summary>
    public const int FROM_INPUT = -1;
    // public static IAxonType Input => InputAxonType.Instance;
    /// <summary>
    /// Creates an unchanging axon: one that does not update its weights.
    /// </summary>
    public static IAxonType CreateImmutable(int length, IReadOnlyList<float> initialWeight)
    {
        return new ImmutableAxonType(length, initialWeight);
    }

    public int Length { get; }
    public IReadOnlyList<float> InitialWeights { get; }
    /// <param name="currentWeights">This should be modified in-place.</param>
    public void UpdateWeights(float[] currentWeights, int timeSinceLastExcitation, float averageTimeBetweenExcitations, int excitationCount, IFeedback feedback);

    /// <summary>
    /// Gets the charge delivered by this axon.
    /// </summary>
    [DebuggerHidden]
    public float GetCharge(float[] weights) => weights[0];

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

public delegate IAxonType? GetAxonConnectionDelegate(int neuronFromIndex, int neuronToIndex);
