namespace JBSnorro.NN;

public sealed class AxonType
{
    public static AxonType Input { get; } = new((_, _) => 1, (_, _) => 1);


    private readonly Func<int, int, int> getLength;
    private readonly Func<int, int, float> getInitialWeight;
    public AxonType(Func<int, int, int> getLength,
                    Func<int, int, float> getInitialWeight)
    {
        this.getLength = getLength;
        this.getInitialWeight = getInitialWeight;
    }
    public int GetLength(int i, int j)
    {
        return this.getLength(i, j);
    }
    public float GetInitialWeight(int i, int j)
    {
        return this.getInitialWeight(i, j);
    }
}
