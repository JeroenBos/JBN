using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface IAxonType
{
    public static IAxonType Input => InputAxonInitialization.Instance;

    public float GetUpdatedWeight(float currentWeight, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, Feedback feedback);
}
