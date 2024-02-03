namespace JBSnorro.NN.Internals;

internal sealed class InputAxonType : IAxonType
{
    public static InputAxonType Instance { get; } = new InputAxonType();

    void IAxonType.UpdateWeights(float[] currentWeight, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, IFeedback feedback)
    {
        throw new InvalidOperationException("Input axons don't update their weights");
    }
}
internal sealed class InputAxonInitialization : IAxonInitialization
{
    internal static InputAxonInitialization Input => Create(new float[1] { 1 });
    public static InputAxonInitialization Create(IReadOnlyList<float> initialWeight)
    {
        return new InputAxonInitialization(initialWeight);
    }


    public int Length => 1;
    public IReadOnlyList<float> InitialWeight { get; private init;  }
    public IAxonType AxonType => InputAxonType.Instance;

    private InputAxonInitialization(IReadOnlyList<float> initialWeight)
    {
        this.InitialWeight = initialWeight ?? throw new ArgumentNullException(nameof(initialWeight));
    }
}
