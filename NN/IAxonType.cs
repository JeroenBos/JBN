using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface IAxonType
{
    /// <summary>
    /// An argument passed to <see cref="GetAxonConnection(int, int)"/> indicating the neuron connected to is a (imagined) input neuron.
    /// </summary>
    public const int FROM_INPUT = -1;

    public static IAxonType Input => InputAxonType.Instance;

    /// <param name="currentWeights">This should be modified in-place.</param>
    public void UpdateWeights(float[] currentWeights, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, Feedback feedback);
    /// <summary>
    /// Gets the charge delivered by this axon.
    /// </summary>
    public float GetCharge(float[] weights) => weights[0];
}

/// <summary>
/// Represents an axon that does not update when given feedback.
/// </summary>
public class UnchangingAxonType : IAxonType
{
    public static UnchangingAxonType Instance { get; } = new UnchangingAxonType();
    private UnchangingAxonType() { }
    public void UpdateWeights(float[] currentWeights, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, Feedback feedback)
    {
        
    }
}
