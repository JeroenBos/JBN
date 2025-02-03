using System.Diagnostics;

namespace JBSnorro.NN.Internals;

/// <summary>
/// Represents an <see cref="IAxonBuilder"/> common for input axons.
/// There is nothing really special about these, they just typically have length 1, never update their weights, and only the first of their initial weights matters.
/// </summary>
[DebuggerDisplay("Input")]
internal sealed class InputAxonType : IAxonBuilder
{
    public static InputAxonType Create(int endNeuronIndex, int length = 1)
    {
        return Create(endNeuronIndex, length, [1f]);
    }
    public static InputAxonType Create(int endNeuronIndex, int length, IReadOnlyList<float> initialWeights)
    {
        IAxonBuilder.AssertPreconditions(length, initialWeights);
        return new InputAxonType(length, initialWeights, endNeuronIndex);
    }

    public int Length { get; }
    public IReadOnlyList<float> InitialWeights { get; }

    public int StartNeuronIndex => IAxonBuilder.FROM_INPUT;
    public int EndNeuronIndex { get; }

    private InputAxonType(int length, IReadOnlyList<float> initialWeights, int endNeuronIndex) => (Length, InitialWeights, EndNeuronIndex) = (length, initialWeights, endNeuronIndex);

    public void UpdateWeights(float[] currentWeight, int timeSinceLastExcitation, float averageTimeBetweenExcitations, int excitationCount, IFeedback feedback)
    {
        throw new InvalidOperationException("Input axons don't update their weights");
    }
}
