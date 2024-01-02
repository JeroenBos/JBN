using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface INeuronType
{
    /// <inheritdoc cref="VariableNeuronType(ValueTuple{int, float}[], ValueTuple{int, float}[])"/>
    public static INeuronType CreateVariable((int maxDt, float decay)[] noActivation, (int maxDt, float decay)[] activation)
    {
        return new VariableNeuronType(noActivation, activation);
    }

    float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastActivation);
}
