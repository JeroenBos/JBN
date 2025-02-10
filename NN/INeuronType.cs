using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface INeuronType
{
    /// <inheritdoc cref="VariableNeuronType(ValueTuple{int, float}[], ValueTuple{int, float}[])"/>
    public static INeuronType CreateVariable((int maxDt, float decay)[] noExcitation, (int maxDt, float decay)[] excitation, float[]? initialCharge = null)
    {
        initialCharge ??= [0];
        return new VariableNeuronType(noExcitation, excitation, initialCharge);
    }
    /// <summary>
    /// Gets a neuron that has no retention time, i.e. could fire the next step directly after it has fired.
    /// </summary>
    public static INeuronType NoRetentionNeuronType { get; } = new RetentionOfOneNeuronType();

    /// <summary>
    /// A bias node.
    /// </summary>
    public static INeuronType AlwaysOn { get; } = new AlwaysOnNeuronType();
    /// <summary>
    /// Gets the decay factors of each of the charge dimensions.
    /// </summary>
    public IReadOnlyList<float> GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastExcitation);

    public IReadOnlyList<float> InitialCharge => [0];

    public float GetEffectiveCharge(IReadOnlyList<float> charges)
    {
        return charges[0];
    }
}
