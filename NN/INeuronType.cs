using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface INeuronType
{
    /// <inheritdoc cref="VariableNeuronType(ValueTuple{int, float}[], ValueTuple{int, float}[])"/>
    public static INeuronType CreateVariable((int maxDt, float decay)[] noExcitation, (int maxDt, float decay)[] excitation)
    {
        return new VariableNeuronType(noExcitation, excitation);
    }
    /// <summary>
    /// Gets a neuron that has no retention time, i.e. could fire the next step directly after it has fired.
    /// </summary>
    public static INeuronType NoRetentionNeuronType { get; } = new RetentionOfOneNeuronType();

    public float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastExcitation);

    public float InitialCharge => 0;
}
