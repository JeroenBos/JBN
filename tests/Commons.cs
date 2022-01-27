using JBSnorro.NN;


static class NeuronTypes
{
    public static NeuronType Empty { get; } = new NeuronType();
    public static NeuronType A { get; } = new NeuronType();
    public static NeuronType[] OnlyA { get; }
    public static NeuronType One { get; } = new NeuronType();
    public static NeuronType[] OnlyOne { get; }

    static NeuronTypes()
    {
        OnlyA = new NeuronType[] { A };
        OnlyOne = new NeuronType[] { One };
    }
}

static class AxonTypes
{
    public static AxonType One { get; } = new AxonType();
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
}

static class GetInitialWeightFunctions
{
    public static float One(int i, int j)
    {
        return 1;
    }
}
