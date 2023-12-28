using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface INeuronType
{
    public static INeuronType CreateVariable((int maxDt, float decay)[] noActivation, (int maxDt, float decay)[] activation)
    {
        return new VariableNeuronType(noActivation, activation);
    }
    float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastActivation);
}
