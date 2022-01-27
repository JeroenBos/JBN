namespace JBSnorro.NN;

public interface INeuronType
{
    internal const int NEVER = int.MinValue + 1;
    float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastActivation);
}
public sealed class RetentionOfOneNeuronType : INeuronType
{
    /// <summary> Gets the decay of the charge given the times since last recept of charge and last activation. </summary>
    public float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastActivation)
    {
        // the NEVER/2 trick effectively puts the max run time at half of int.MaxValue. Fine for now
        if (INeuronType.NEVER / 2 < timeSinceLastChargeReceipt && timeSinceLastChargeReceipt < 0)
            throw new ArgumentOutOfRangeException(nameof(timeSinceLastChargeReceipt));

        if (INeuronType.NEVER / 2 < timeSinceLastActivation && timeSinceLastActivation < 0)
            throw new ArgumentOutOfRangeException(nameof(timeSinceLastActivation));

        if (timeSinceLastActivation == 1)
            return 0;
        return 1;
    }
}

public sealed class VariableNeuronType : INeuronType
{
    const float tailValue = 0;
    private readonly (int maxDt, float decay)[] noActivation;
    private readonly (int maxDt, float decay)[] activation;

    /// <summary> 
    /// The maxDT should be thought of as number of time steps until and inclusing which the associated decay rate applies. 
    /// For example, a maxDT of 2 will apply 
    /// </summary>
    public VariableNeuronType((int maxDt, float decay)[] noActivation, (int maxDt, float decay)[] activation)
    {
        for (int i = 0; i < noActivation.Length - 1; i++)
        {
            if (noActivation[i].maxDt >= noActivation[i + 1].maxDt)
                throw new ArgumentException("maxDts must be increasing", nameof(noActivation));
        }

        for (int i = 0; i < activation.Length - 1; i++)
        {
            if (activation[i].maxDt >= activation[i + 1].maxDt)
                throw new ArgumentException("maxDts must be increasing", nameof(activation));
        }

        this.noActivation = noActivation;
        this.activation = activation;
    }
    public float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastActivation)
    {
        int dt;
        (int max, float decay)[] list;
        if (timeSinceLastChargeReceipt >= timeSinceLastActivation && timeSinceLastActivation >= INeuronType.NEVER / 2)
        {
            list = this.activation;
            dt = timeSinceLastActivation;
        }
        else if(timeSinceLastChargeReceipt >= INeuronType.NEVER / 2)
        {
            list = this.noActivation;
            dt = timeSinceLastChargeReceipt;
        }
        else // no charge ever received (nor activation)
        {
            return 1;
        }

        foreach (var (maxDt, decay) in list)
        {
            if (maxDt >= dt)
            {
                return decay;
            }
        }
        return tailValue;
    }
    internal IEnumerable<float> GetActivationDecaySequence()
    {
        return GetDecaySequence(this.activation);
    }
    internal IEnumerable<float> GetNoActivationDecaySequence()
    {
        return GetDecaySequence(this.noActivation);
    }
    private IEnumerable<float> GetDecaySequence((int maxDt, float decay)[] decays)
    {
        int currentIndex = 0;
        int currentMaxDt = decays[currentIndex].maxDt;
        float currentValue = decays[currentIndex].decay;
        foreach (var t in Enumerable.Range(1, int.MaxValue))
        {
            if (t <= currentMaxDt)
            {
                yield return currentValue;
            }
            else if (currentIndex + 1 >= decays.Length)
            {
                currentMaxDt = int.MaxValue;
                currentValue = tailValue;
            }
            else
            {
                currentIndex++;
                currentMaxDt = decays[currentIndex].maxDt;
                currentValue = decays[currentIndex].decay;
            }
        }
    }
    internal IEnumerable<float> GetActivationCumulativeDecaySequence()
    {
        return GetDecaySequence(this.activation).Scan(floatMultiplication, 1);
    }
    internal IEnumerable<float> GetNoActivationCumulativeDecaySequence()
    {
        return GetDecaySequence(this.noActivation).Scan(floatMultiplication, 1);
    }
    static float floatMultiplication(float a, float b) => a * b;
}
