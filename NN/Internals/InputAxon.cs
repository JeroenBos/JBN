namespace JBSnorro.NN.Internals;

/// <summary>
/// Represents an <see cref="IAxonType"/> common for input axons.
/// There is nothing really special about these, they just typically have length 1, never update their weights, and only the first of their initial weights matters.
/// </summary>
internal sealed class InputAxonType : BaseAxonType, IAxonType
{
    public static InputAxonType Instance { get; } = Create(initialWeights: new float[] { 1 });
    public static InputAxonType Create(IReadOnlyList<float> initialWeights, int length = 1)
    {
        return new InputAxonType(length, initialWeights);
    }

    private InputAxonType(int length, IReadOnlyList<float> initialWeights) : base(length, initialWeights) { }

    public override void UpdateWeights(float[] currentWeight, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, IFeedback feedback)
    {
        throw new InvalidOperationException("Input axons don't update their weights");
    }
}
