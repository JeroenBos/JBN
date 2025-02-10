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
    public static InputAxonType Create(object endNeuronIndex, int length, IReadOnlyList<float> initialWeights)
    {
        IAxonBuilder.AssertPreconditions(length, initialWeights);
        return new InputAxonType(length, initialWeights, endNeuronIndex);
    }

    public int Length { get; }
    public IReadOnlyList<float> InitialWeights { get; }

    public object StartNeuronLabel => IAxonBuilder.FromInputLabel;
    public object EndNeuronLabel { get; }

    private InputAxonType(int length, IReadOnlyList<float> initialWeights, object endNeuronIndex) => (Length, InitialWeights, EndNeuronLabel) = (length, initialWeights, endNeuronIndex);
}
