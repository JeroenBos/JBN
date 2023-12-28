using JBSnorro.NN.Internals;
using JBSnorro.NN;

namespace Tests.JBSnorro.NN;

static class NeuronTypes
{
    public static INeuronType A { get; }
    public static INeuronType B { get; }
    public static INeuronType C { get; }
    public static INeuronType[] OnlyA { get; }
    public static INeuronType One { get; } = new RetentionOfOneNeuronType();
    public static INeuronType[] OnlyOne { get; }

    static NeuronTypes()
    {
        float sqrt2 = (float)Math.Sqrt(2);
        float invSqrt2 = 1 / sqrt2;
        A = new VariableNeuronType(new[] {
            (10, 0.9f),
            (11, 0f),
        }, new (int, float)[] {
            (0, 1f),
        });

        B = new VariableNeuronType(new (int, float)[] {
            // does not decay
        }, new[] {
            (1, 0.1f),
        });

        C = new VariableNeuronType(new[] {
            (1, 0.9f),
        }, new[] {
            (1, 0.1f),
            (2, sqrt2),
            (3, sqrt2),
            (4, 1),
            (5, invSqrt2),
            (6, invSqrt2),
        });

        OnlyA = new[] { A };
        OnlyOne = new[] { One };
    }
}

static class AxonTypes
{
    public static AxonType A { get; } = new AxonType(type: 1, length: 1, initialWeight: 1);
    public static AxonType LengthTwo { get; } = new AxonType(type: 2, length: 2, initialWeight: 1);
}

class Machines
{
    public static Machine AtTime0 { get; }
    static Machines()
    {
        AtTime0 = new Machine(INetwork.Create(Array.Empty<INeuronType>(), 0, 0, new AxonType[0, 0], INetworkInitializer.CreateRandom(seed: 0), null));
        AtTime0.Run(0);
    }
}
