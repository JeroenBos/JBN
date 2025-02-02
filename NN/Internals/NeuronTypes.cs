using System.Runtime.CompilerServices;

namespace JBSnorro.NN.Internals;

internal sealed class RetentionOfOneNeuronType : INeuronType
{
    public float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastExcitation)
    {
        Assert(IsNever(timeSinceLastChargeReceipt) || timeSinceLastChargeReceipt >= 0, $"{nameof(timeSinceLastChargeReceipt)} out of range");
        Assert(IsNever(timeSinceLastExcitation) || timeSinceLastExcitation >= 0, $"{nameof(timeSinceLastExcitation)} out of range");

        // return 0 means charge immediately decays, but it still had the chance to elicit an excitation
        return 0;
    }
}

internal sealed class VariableNeuronType : INeuronType
{
    const float ExcitationTailValue = 1;
    const float ChargeTailValue = 0;
    private readonly (int maxDt, float decay)[] noExcitation;
    private readonly (int maxDt, float decay)[] excitation;

    /// <summary>
    /// Creates a <see cref="INeuronType"/> where the excitations are variable.
    /// </summary>
    /// <param name="noExcitation">
    /// The maxDt should be thought of as number of time steps until and including which the associated decay rate applies. 
    /// </param>
    public VariableNeuronType((int maxDt, float decay)[] noExcitation, (int maxDt, float decay)[] excitation)
    {
        verify(noExcitation);
        verify(excitation);

        this.noExcitation = noExcitation;
        this.excitation = excitation;

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

    public float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastExcitation)
    {
        // we can assume we're at the end of a time step
        bool hasReceivedCharge = !IsNever(timeSinceLastChargeReceipt);
        bool hasExcited = !IsNever(timeSinceLastExcitation);
        bool excitationWasLaterThanChargeReceipt = timeSinceLastChargeReceipt >= timeSinceLastExcitation;


        int dt;
        float tail;
        (int max, float decay)[] list;
        if (hasExcited)
        {
            Assert(hasReceivedCharge);

            list = this.excitation;
            dt = timeSinceLastExcitation;
            tail = ExcitationTailValue;
        }
        else if (!hasReceivedCharge)
        {
            Assert(!hasExcited);

            // In this case the current decay _must_ be 0, so we can return anything. 
            // 0 would make any mistake stand out.
            return 0;
        }
        else // has received charge without activation
        {
            list = this.noExcitation;
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
    internal IEnumerable<float> GetExcitationDecaySequence()
    {
        return GetDecaySequence(this.excitation, ExcitationTailValue);
    }
    internal IEnumerable<float> GetNoExcitationDecaySequence()
    {
        return GetDecaySequence(this.noExcitation, ChargeTailValue);
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
    internal IEnumerable<float> GetExcitationCumulativeDecaySequence()
    {
        return GetExcitationDecaySequence().Fold((a, b) => a * b, 1);
    }
    internal IEnumerable<float> GetNoExcitationCumulativeDecaySequence()
    {
        return GetNoExcitationDecaySequence().Fold((a, b) => a * b, 1);
    }
}


internal sealed class AlwaysOnNeuronType : INeuronType
{
    public float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastExcitation)
    {
        Assert(IsNever(timeSinceLastChargeReceipt) || timeSinceLastChargeReceipt >= 0, $"{nameof(timeSinceLastChargeReceipt)} out of range");
        Assert(IsNever(timeSinceLastExcitation) || timeSinceLastExcitation >= 0, $"{nameof(timeSinceLastExcitation)} out of range");

        // return 0 means charge never decays, but it still had the chance to elicit an excitation
        return 1;
    }
    float INeuronType.InitialCharge => 1;
}
