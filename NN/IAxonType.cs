using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface IAxonType
{
    public static IAxonType Input { get; } = new AxonType(type: 0, length: 1, initialWeight: 1);


    /// <summary>
    /// Gets the length of the axon between the <paramref name="i"/>th and <paramref name="j"/>th neurons.
    /// </summary>
    /// <returns>
    /// The length; can be negative. For unconnected neurons, a <see langword="null"/> <see cref="IAxonType"/> should have been specified.
    /// </returns>
    public int GetLength(int i, int j);
    /// <summary>
    /// Gets the initial weight of the axon between the <paramref name="i"/>th and <paramref name="j"/>th neurons.
    /// </summary>
    public float GetInitialWeight(int i, int j);
    public float GetUpdatedWeight(float currentWeight, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, Feedback feedback);
}
