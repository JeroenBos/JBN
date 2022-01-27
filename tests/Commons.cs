using JBSnorro.NN;


static class NeuronTypes
{
    public static INeuronType A { get; }
    public static INeuronType[] OnlyA { get; }
    public static INeuronType One { get; } = new RetentionOfOneNeuronType();
    public static INeuronType[] OnlyOne { get; }

    static NeuronTypes()
    {
        OnlyA = new INeuronType[] { A };
        OnlyOne = new INeuronType[] { One };
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
