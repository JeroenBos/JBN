using JBSnorro.NN;
using JBSnorro.NN.Internals;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace Tests.JBSnorro.NN;

static class NeuronTypes
{
    public static INeuronType A { get; }
    public static INeuronType B { get; }
    public static INeuronType C { get; }
    public static INeuronType[] OnlyA { get; }
    public static INeuronType One { get; } = INeuronType.NoRetentionNeuronType;
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

class MockAxonType : IAxonType
{
    public static IAxonType LengthTwo { get; } = new MockAxonType(length: 2, initialWeights: new float[] { 1 });

    public static IAxonType?[,] CreateRandom(int neuronCount, float connectionChance, Random random)
    {
        var result = new IAxonType?[neuronCount, neuronCount];
        var getLength = CreateDefault2DGetLength(neuronCount);
        var getInitialWeight = CreateRandomWeightInitializer(random);
        for (int i = 0; i < neuronCount; i++)
        {
            for (int j = 0; j < neuronCount; j++)
            {
                if (random.NextSingle() < connectionChance)
                {
                    result[i, j] = new MockAxonType(getLength(i, j), new float[] { getInitialWeight(i, j) });
                }
            }
        }
        return result;
    }
    private static Func<int, int, int> CreateDefault2DGetLength(int total, int speed = 4)
    {
        return (i, j) => Default2DGetLength(i, j, total, speed);
    }
    private static int Default2DGetLength(int i, int j, int total, int speed = 4)
    {
        int side = (int)Math.Max(1, Math.Sqrt(total));
        int ix = Math.DivRem(i, side, out int iy);
        int jx = Math.DivRem(j, side, out int jy);
        int dx = ix - jx;
        int dy = iy - jy;
        var result = (int)Math.Max(1, Math.Sqrt(dx * dx + dy * dy) / speed);
        return result;
    }
    private static Func<int, int, float> CreateRandomWeightInitializer(Random random)
    {
        return f;
        float f(int i, int j)
        {
            return 2 * random.NextSingle() - 0.9f;
        }
    }

    private MockAxonType(int length, float[] initialWeights) => (this.Length, this.InitialWeights) = (length, initialWeights);
    public int Length { get; }
    public IReadOnlyList<float> InitialWeights { get; }
    public void UpdateWeights(float[] currentWeight, int timeSinceLastActivation, float averageTimeBetweenActivations, int activationCount, IFeedback feedback)
    {
    }
}

class Machines
{
    public static Machine AtTime0 { get; }
    static Machines()
    {
        AtTime0 = new Machine(INetwork.Create(Array.Empty<INeuronType>(), 0, (i, j) => null, IClock.Create(null)), (_, _) => null);
        AtTime0.Run(0);
    }
}
