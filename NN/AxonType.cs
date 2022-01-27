namespace JBSnorro.NN;

public sealed class AxonType
{
    public delegate float GetUpdatedWeightDelegate(float weight, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, float dopamine, float cortisol);
    public static float DefaultGetUpdatedWeightDelegate(float weight, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, float dopamine, float cortisol)
    {
        return weight;
    }
    public static AxonType Input { get; } = new((_, _) => 1, (_, _) => 1, DefaultGetUpdatedWeightDelegate);


    private readonly Func<int, int, int> getLength;
    private readonly Func<int, int, float> getInitialWeight;
    private readonly GetUpdatedWeightDelegate getUpdatedWeight;

    public AxonType(Func<int, int, int> getLength, Func<int, int, float> getInitialWeight)
      : this(getLength, getInitialWeight, DefaultGetUpdatedWeightDelegate) { }
    public AxonType(Func<int, int, int> getLength,
                    Func<int, int, float> getInitialWeight,
                    GetUpdatedWeightDelegate getUpdatedWeight)
    {
        this.getLength = getLength;
        this.getInitialWeight = getInitialWeight;
        this.getUpdatedWeight = getUpdatedWeight;
    }
    public int GetLength(int i, int j)
    {
        return this.getLength(i, j);
    }
    public float GetInitialWeight(int i, int j)
    {
        return this.getInitialWeight(i, j);
    }
    public float GetUpdatedWeight(float currentWeight,
                                  int timeSinceLastActivation,
                                  float averageTimeBetweenActivations,
                                  int activationCount,
                                  float dopamine,
                                  float cortisol)
    {
        return getUpdatedWeight(currentWeight,
                                timeSinceLastActivation,
                                averageTimeBetweenActivations,
                                activationCount,
                                dopamine,
                                cortisol);
    }
}
