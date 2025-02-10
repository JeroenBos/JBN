using JBSnorro.NN;
using JBSnorro.NN.Internals;

namespace Tests.JBSnorro.NN;

internal static class AxonBuilders
{
    public static IAxonBuilder LengthTwo(int startNeuronIndex, int endNeuronIndex) => new ImmutableAxonBuilder(length: 2, initialWeights: [1f], startNeuronIndex, endNeuronIndex);
    public static IAxonBuilder Default(int startNeuronIndex, int endNeuronIndex) => new ImmutableAxonBuilder(length: 1, initialWeights: [1f], startNeuronIndex, endNeuronIndex);
    public static IAxonBuilder?[,] CreateRandom(int neuronCount, float connectionChance, Random random)
    {
        var result = new IAxonBuilder?[neuronCount, neuronCount];
        var getLength = CreateDefault2DGetLength(neuronCount);
        var getInitialWeight = CreateRandomWeightInitializer(random);
        for (int i = 0; i < neuronCount; i++)
        {
            for (int j = 0; j < neuronCount; j++)
            {
                if (random.NextSingle() < connectionChance)
                {
                    result[i, j] = new ImmutableAxonBuilder(length: getLength(i, j), [getInitialWeight(i, j)], i, j);
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
}
