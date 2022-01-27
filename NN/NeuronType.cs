using System.Runtime.CompilerServices;

namespace JBSnorro.NN;

public interface INeuronType
{
    internal static bool IsNever(int i)
    {
        // the int.MinValue/2 trick effectively puts the max run time at half of int.MaxValue. Fine for now
        return i < int.MinValue / 2;
    }
    float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastActivation);
}
public sealed class RetentionOfOneNeuronType : INeuronType
{
    /// <summary> Gets the decay of the charge given the times since last recept of charge and last activation. </summary>
    public float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastActivation)
    {
        if (!INeuronType.IsNever(timeSinceLastChargeReceipt) && timeSinceLastChargeReceipt < 0)
            throw new ArgumentOutOfRangeException(nameof(timeSinceLastChargeReceipt));

        if (!INeuronType.IsNever(timeSinceLastActivation) && timeSinceLastActivation < 0)
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
        verify(noActivation);
        verify(activation);

        this.noActivation = noActivation;
        this.activation = activation;

        static void verify((int maxDt, float decay)[] list, [CallerArgumentExpression("list")] string paramName = "")
        {
            for (int i = 0; i < list.Length - 1; i++)
            {
                if (list[i].maxDt >= list[i + 1].maxDt)
                    throw new ArgumentException("maxDts must be increasing", paramName);
            }
            if (list.Length != 0 && list[0].maxDt < 0)
                throw new ArgumentException("maxDts must be nonnegative", paramName);
        }
    }
    public float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastActivation)
    {
        // we can assume we're at the end of a time
        bool hasReceivedCharge = !INeuronType.IsNever(timeSinceLastChargeReceipt);
        bool hasActivated = !INeuronType.IsNever(timeSinceLastActivation);
        bool activationWasLaterThanChargeReceipt = timeSinceLastChargeReceipt >= timeSinceLastActivation;


        int dt;
        (int max, float decay)[] list;
        if (hasActivated)
        {
            Assert(hasReceivedCharge);

            list = this.activation;
            dt = timeSinceLastActivation;
        }
        else if (!hasReceivedCharge)
        {
            Assert(!hasActivated);

            // In this case the current decay _must_ be 0, so we can return anything. 
            // 0 would make any mistake stand out.
            return 0;
        }
        else // has received charge without activation
        {
            list = this.noActivation;
            dt = timeSinceLastChargeReceipt;
        }


        if (dt == 0)
        {
            // dt == 0 is a special case which is assumed to always return 1, unless explicitly stated otherwise (through ctor arguments)
            if (list.Length == 0 || list[0].max != 0)
            {
                return 1;
            }
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
        return GetActivationDecaySequence().Scan(floatMultiplication, 1);
    }
    internal IEnumerable<float> GetNoActivationCumulativeDecaySequence()
    {
        return GetNoActivationDecaySequence().Scan(floatMultiplication, 1);
    }
    static float floatMultiplication(float a, float b) => a * b;
}
