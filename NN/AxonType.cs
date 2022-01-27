namespace JBSnorro.NN;

public sealed class AxonType
{
    public static AxonType Input { get; } = new(type: 0, length: 1, initialWeight: 1);

    private readonly byte type;
    private readonly Func<int, int, int> getLength;
    private readonly Func<byte, int, int, float> getInitialWeight;
    private readonly GetUpdatedWeightDelegate getUpdatedWeight;

    internal AxonType(byte type, int length, int initialWeight) : this(type, (_, _) => length, (_, _, _) => initialWeight) { }
    public AxonType(byte type, Func<int, int, int> getLength, Func<byte, int, int, float> getInitialWeight)
      : this(type, getLength, getInitialWeight, DefaultGetUpdatedWeightDelegate) { }
    public AxonType(byte type,
                    Func<int, int, int> getLength,
                    Func<byte, int, int, float> getInitialWeight,
                    GetUpdatedWeightDelegate getUpdatedWeight)
    {
        this.type = type;
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
        return this.getInitialWeight(this.type, i, j);
    }
    public float GetUpdatedWeight(float currentWeight,
                                  int timeSinceLastActivation,
                                  float averageTimeBetweenActivations,
                                  int activationCount,
                                  float dopamine,
                                  float cortisol)
    {
        return getUpdatedWeight(currentWeight,
                                this.type,
                                timeSinceLastActivation,
                                averageTimeBetweenActivations,
                                activationCount,
                                dopamine,
                                cortisol);
    }


    public delegate float GetUpdatedWeightDelegate(float weight, byte type, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, float dopamine, float cortisol);

    // some default delegate implementations
    private static float DefaultGetUpdatedWeightDelegate(float weight, byte type, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, float dopamine, float cortisol)
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

    public static Func<byte, int, int, float> CreateRandomWeightInitializer(Random random)
    {
        return f;
        float f(byte type, int i, int j)
        {
            return 2 * random.NextSingle() - 0.9f;
        }
    }
}
