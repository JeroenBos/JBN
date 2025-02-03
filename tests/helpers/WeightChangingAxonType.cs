using JBSnorro.NN;

sealed class WeightChangingAxonType<TFeedback>(Action<float[], TFeedback> changeWeights) : IAxonBuilder where TFeedback : IFeedback
{
    public int Length => 1;
    public IReadOnlyList<float> InitialWeights => [1f];

    public void UpdateWeights(float[] currentWeights, int timeSinceLastExcitation, float averageTimeBetweenExcitations, int excitationCount, IFeedback feedback)
    {
        changeWeights(currentWeights, (TFeedback)feedback);
    }
}
