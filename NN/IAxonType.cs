using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface IAxonType
{
    public static IAxonType Input => InputAxonType.Instance;

    /// <param name="currentWeights">This should be modified in-place.</param>
    public void UpdateWeights(float[] currentWeights, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, Feedback feedback);
    /// <summary>
    /// Gets the charge delivered by this axon.
    /// </summary>
    public float GetCharge(float[] weights) => weights[0];
}
