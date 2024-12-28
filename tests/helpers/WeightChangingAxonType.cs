using JBSnorro.NN;

sealed class WeightChangingAxonType(Action<float[]> changeWeights) : IAxonType
{
    private readonly Action<float[]> changeWeights = changeWeights;
    public int Length => 1;
    public IReadOnlyList<float> InitialWeights => [1f];

    public void UpdateWeights(float[] currentWeights, int timeSinceLastExcitation, float averageTimeBetweenExcitations, int excitationCount, IFeedback feedback)
    {
        this.changeWeights(currentWeights);
    }
}
