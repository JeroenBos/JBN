using JBSnorro.NN;

sealed class WeightChangingAxonType<TFeedback>(Action<float[], TFeedback> changeWeights, int startNeuronIndex, int endNeuronIndex) : IAxonBuilder where TFeedback : IFeedback
{
    public int Length => 1;
    public IReadOnlyList<float> InitialWeights => [1f];

    public int StartNeuronIndex => startNeuronIndex;
    public int EndNeuronIndex => endNeuronIndex;

    public void UpdateWeights(float[] currentWeights, int timeSinceLastExcitation, float averageTimeBetweenExcitations, int excitationCount, IFeedback feedback)
    {
        changeWeights(currentWeights, (TFeedback)feedback);
    }
}
