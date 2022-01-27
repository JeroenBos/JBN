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

}
static class GetLengthFunctions
{
    public static int One(int i, int j)
    {
        return 1;
    }
    public static int Two(int i, int j)
    {
        return 2;
    }
    public static int Default(int i, int j)
    {
        return (int)Math.Max(1, Math.Sqrt(i * i + j * j) / 4);
    }
}

static class GetInitialWeightFunctions
{
    public static float One(int i, int j)
    {
        return 1;
    }
    public static Func<int, int, float> Random(Random random)
    {
        return f;
        float f(int i, int j)
        {
            return 2 * random.NextSingle() - 1;
        }
    }
}
