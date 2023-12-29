namespace JBSnorro.NN.Internals;

internal sealed class InputAxonInitialization : IAxonInitialization, IAxonType
{
    public static InputAxonInitialization Instance { get; } = new InputAxonInitialization();

    public int Length => 1;
    public float InitialWeight => 1;
    public IAxonType AxonType => this;

    private InputAxonInitialization() { }

    float IAxonType.GetUpdatedWeight(float currentWeight, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, Feedback feedback)
    {
        throw new InvalidOperationException("Input axons don't update their weights");
    }
}
