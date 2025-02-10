using JBSnorro.NN;
using JBSnorro.NN.Internals;

namespace Tests.JBSnorro.NN;

static class NeuronTypes
{
    public static INeuronType A { get; }
    public static INeuronType B { get; }
    public static INeuronType C { get; }
    public static INeuronType[] OnlyA { get; }
    public static INeuronType One { get; } = INeuronType.NoRetentionNeuronType;
    public static INeuronType[] OnlyOne { get; }
    public static INeuronType InitiallyCharged { get; }
    public static INeuronType AlwaysOn => INeuronType.AlwaysOn;
    public static INeuronType NoRetention => INeuronType.NoRetentionNeuronType;

    static NeuronTypes()
    {
        float sqrt2 = (float)Math.Sqrt(2);
        float invSqrt2 = 1 / sqrt2;
        A = new VariableNeuronType([
            (10, 0.9f),
            (11, 0f),
        ], [
            (0, 1f),
        ], [0]);

        B = new VariableNeuronType([], [(1, 0.1f)], [0]);

        C = new VariableNeuronType([
            (1, 0.9f),
        ], [
            (1, 0.1f),
            (2, sqrt2),
            (3, sqrt2),
            (4, 1),
            (5, invSqrt2),
            (6, invSqrt2),
        ], [0]);

        OnlyA = [A];
        OnlyOne = [One];

        InitiallyCharged = new InitiallyChargedNeuronType(INeuronType.NoRetentionNeuronType, initialCharge: [1f]);
    }
}


class InitiallyChargedNeuronType(INeuronType adaptee, IReadOnlyList<float> initialCharge) : INeuronType
{
    private readonly INeuronType adaptee = adaptee ?? throw new ArgumentNullException(nameof(adaptee));
    IReadOnlyList<float> INeuronType.InitialCharge { get; } = initialCharge;

    public IReadOnlyList<float> GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastExcitation)
    {
        return adaptee.GetDecay(timeSinceLastChargeReceipt, timeSinceLastExcitation);
    }
}
class Machines
{
    public static Machine AtTime0 { get; }
    static Machines()
    {
        AtTime0 = new Machine(INetwork.Create([], 0,  (_, _, _, _, _, _, _) => {}, IClock.Create(null)));
        AtTime0.Run(0);
    }
}
