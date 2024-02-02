using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface INeuronType
{
    /// <inheritdoc cref="VariableNeuronType(ValueTuple{int, float}[], ValueTuple{int, float}[])"/>
    public static INeuronType CreateVariable((int maxDt, float decay)[] noActivation, (int maxDt, float decay)[] activation)
    {
        return new VariableNeuronType(noActivation, activation);
    }
    /// <summary>
    /// Gets a neuron that has no retention time, i.e. could fire the next step directly after it has fired.
    /// </summary>
    public static INeuronType NoRetentionNeuronType { get; } = new RetentionOfOneNeuronType();

    public float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastActivation);
}
