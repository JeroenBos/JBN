using JBSnorro.NN;


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
    public static AxonType A { get; } = new AxonType((_, _) => 1, (_, _) => 1);
    public static AxonType LengthTwo { get; } = new AxonType((_, _) => 2, (_, _) => 1);

}
static class GetLengthFunctions
{
    public static Func<int, int, int> CreateDefault(int total, int speed = 4)
    {
        return (i, j) => Default(i, j, total, speed);
    }
    public static int Default(int i, int j, int total, int speed = 4)
    {
        int side = (int)Math.Max(1, Math.Sqrt(total));
        int ix = Math.DivRem(i, side, out int iy);
        int jx = Math.DivRem(j, side, out int jy);
        int dx = ix - jx;
        int dy = iy - jy;
        var result = (int)Math.Max(1, Math.Sqrt(dx * dx + dy * dy) / speed);
        return result;
    }
}

static class GetInitialWeightFunctions
{
    public static Func<int, int, float> Random(Random random)
    {
        return f;
        float f(int i, int j)
        {
            return 2 * random.NextSingle() - 0.9f;
        }
    }
}

class Machines
{
    public static Machine AtTime0 { get; }
    static Machines()
    {
        AtTime0 = new Machine(new Network(Array.Empty<INeuronType>(), 0, 0, new AxonType[0, 0]));
        AtTime0.Run(0);
    }
}
