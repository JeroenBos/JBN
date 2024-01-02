using System.Runtime.CompilerServices;

namespace JBSnorro.NN.Internals;

internal sealed class RetentionOfOneNeuronType : INeuronType
{
    /// <summary> Gets the decay of the charge given the times since last receipt of charge and last activation. </summary>
    public float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastActivation)
    {
        Assert(IsNever(timeSinceLastChargeReceipt) || timeSinceLastChargeReceipt >= 0, $"{nameof(timeSinceLastChargeReceipt)} out of range");
        Assert(IsNever(timeSinceLastActivation) || timeSinceLastActivation >= 0, $"{nameof(timeSinceLastActivation)} out of range");

        // return 0 means charge immediately decays, but it still had the chance to elicit an excitation
        return 0;
    }
}

internal sealed class VariableNeuronType : INeuronType
{
    const float ActivationTailValue = 1;
    const float ChargeTailValue = 0;
    private readonly (int maxDt, float decay)[] noActivation;
    private readonly (int maxDt, float decay)[] activation;

    /// <summary>
    /// Creates a <see cref="INeuronType"/> where the activations are variable.
    /// </summary>
    /// <param name="noActivation">
    /// The maxDt should be thought of as number of time steps until and inclusing which the associated decay rate applies. 
    /// </param>
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
        // we can assume we're at the end of a time step
        bool hasReceivedCharge = !IsNever(timeSinceLastChargeReceipt);
        bool hasActivated = !IsNever(timeSinceLastActivation);
        bool activationWasLaterThanChargeReceipt = timeSinceLastChargeReceipt >= timeSinceLastActivation;


        int dt;
        float tail;
        (int max, float decay)[] list;
        if (hasActivated)
        {
            Assert(hasReceivedCharge);

            list = this.activation;
            dt = timeSinceLastActivation;
            tail = ActivationTailValue;
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
            tail = ChargeTailValue;
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
        return tail;
    }
    internal IEnumerable<float> GetActivationDecaySequence()
    {
        return GetDecaySequence(this.activation, ActivationTailValue);
    }
    internal IEnumerable<float> GetNoActivationDecaySequence()
    {
        return GetDecaySequence(this.noActivation, ChargeTailValue);
    }
    private IEnumerable<float> GetDecaySequence((int maxDt, float decay)[] decays, float tailValue)
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
        return GetActivationDecaySequence().Scan((a, b) => a * b, 1);
    }
    internal IEnumerable<float> GetNoActivationCumulativeDecaySequence()
    {
        return GetNoActivationDecaySequence().Scan((a, b) => a * b, 1);
    }
}
