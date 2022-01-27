namespace JBSnorro.NN;

public sealed class AxonType
{
    public static AxonType Input { get; } = new();


    private readonly int length;
    private readonly float initialWeight;
    public AxonType(int length = 1, float initialWeight = 1)
    {
        this.length = length;
        this.initialWeight = initialWeight;
    }
    public int GetLength(int i, int j)
    {
        return this.length;
    }
    public float GetInitialWeight(int i, int j)
    {
        return this.initialWeight;
    }
}
