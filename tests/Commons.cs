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
        A = new VariableNeuronType(new[] {
            (1, 1f),
            (int.MaxValue, 1f),
        }, new[] {
            (1, 1),
            (int.MaxValue, 1f),
        });

        B = new VariableNeuronType(new[] {
            (1, 1f),
            (int.MaxValue, 1f),
        }, new[] {
            (1, 1),
            (int.MaxValue, 1f),
        });

        C = new VariableNeuronType(new[] {
            (1, 1f),
            (int.MaxValue, 1f),
        }, new[] {
            (1, 1),
            (int.MaxValue, 1f),
        });

        OnlyA = new[] { A };
        OnlyOne = new[] { One };
    }
}

static class AxonTypes
{
    public static AxonType A { get; } = new AxonType();
    public static AxonType LengthTwo { get; } = new AxonType(2);

}
static class GetLengthFunctions
{
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
            return 2 * random.NextSingle() - 1;
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
