namespace JBSnorro.NN.Internals;

/// <summary>
/// Represents an <see cref="IAxonType"/> common for input axons.
/// There is nothing really special about these, they just typically have length 1, never update their weights, and only the first of their initial weights matters.
/// </summary>
internal sealed class InputAxonType : IAxonType
{
    public static InputAxonType Instance { get; } = Create(initialWeights: [1f]);
    public static InputAxonType Create(IReadOnlyList<float> initialWeights, int length = 1)
    {
        IAxonType.AssertPreconditions(length, initialWeights);
        return new InputAxonType(length, initialWeights);
    }

    public int Length { get; }
    public IReadOnlyList<float> InitialWeights { get; }

    private InputAxonType(int length, IReadOnlyList<float> initialWeights) => (Length, InitialWeights) = (length, initialWeights);

    public void UpdateWeights(float[] currentWeight, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, IFeedback feedback)
    {
        throw new InvalidOperationException("Input axons don't update their weights");
    }
}
