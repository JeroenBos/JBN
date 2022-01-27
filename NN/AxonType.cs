namespace JBSnorro.NN;

public sealed class AxonType
{
    public static AxonType Input { get; } = new(length: 1, initialWeight: 1);


    private readonly Func<int, int, int> getLength;
    private readonly Func<int, int, float> getInitialWeight;
    private readonly GetUpdatedWeightDelegate getUpdatedWeight;

    internal AxonType(int length, int initialWeight) : this((_, _) => length, (_, _) => initialWeight) { }
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


    public delegate float GetUpdatedWeightDelegate(float weight, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, float dopamine, float cortisol);

    // some default delegate implementations
    private static float DefaultGetUpdatedWeightDelegate(float weight, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, float dopamine, float cortisol)
    {
        return weight;
    }
    public static Func<int, int, int> CreateDefault2DGetLength(int total, int speed = 4)
    {
        return (i, j) => Default2DGetLength(i, j, total, speed);
    }
    public static int Default2DGetLength(int i, int j, int total, int speed = 4)
    {
        int side = (int)Math.Max(1, Math.Sqrt(total));
        int ix = Math.DivRem(i, side, out int iy);
        int jx = Math.DivRem(j, side, out int jy);
        int dx = ix - jx;
        int dy = iy - jy;
        var result = (int)Math.Max(1, Math.Sqrt(dx * dx + dy * dy) / speed);
        return result;
    }

    public static Func<int, int, float> CreateRandomWeightInitializer(Random random)
    {
        return f;
        float f(int i, int j)
        {
            return 2 * random.NextSingle() - 0.9f;
        }
    }
}
