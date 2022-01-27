namespace JBSnorro.NN;

public class Network
{
    void Assert(bool condition)
    {
        if (!condition) throw new Exception();
    }
    public Network(NeuronType[] nodeTypes,
                   int inputCount,
                   int outputCount,
                   AxonType?[,] connections,
                   Func<int, int, int> getLength,
                   Func<int, int, float> getInitialWeight)
    {
        int nodeCount = nodeTypes.Length;
        Assert(connections.GetLength(0) == nodeCount);
        Assert(connections.GetLength(1) == nodeCount);
        Assert(inputCount <= nodeCount);
        Assert(outputCount <= nodeCount);
        Assert(nodeTypes.All(t => t != null));


        this.nodeTypes = nodeTypes;
        this.axonTypes = connections.Unique().Where(c => c != null).ToArray()!;
        this.nodes = new Neuron[nodeCount];
        this.Input = new Axon[inputCount];
        this.outputCount = outputCount;

        for (int i = 0; i < nodeCount; i++)
        {
            int axonCount = 0;
            for (int j = 0; j < nodeCount; j++)
            {
                if (connections[i, j] != null)
                {
                    axonCount++;
                }
            }
            this.nodes[i] = new Neuron(nodeTypes[i], axonCount);
        }
        for (int i = 0; i < nodeCount; i++)
        {
            for (int j = 0; j < nodeCount; j++)
            {
                var connection = connections[i, j];
                if (connection != null)
                {
                    this.nodes[i].AddAxon(new Axon(connection, this.nodes[j], getLength(i, j), getInitialWeight(i, j)));
                }
            }
        }

        for (int i = 0; i < inputCount; i++)
        {
            this.Input[i] = new Axon(AxonType.Input, this.nodes[i], length: Axon.InputLength);
        }
    }

    private readonly AxonType[] axonTypes;
    private readonly NeuronType[] nodeTypes;
    private readonly Neuron[] nodes;
    public Axon[] Input { get; }
    private readonly int outputCount;
    internal float[] output
    {
        get
        {
            float[] result = new float[this.outputCount];
            for (int i = 0; i < result.Length; i++)
                result[i] = nodes[nodes.Length - outputCount + i].Charge;
            return result;
        }
    }
    public void WriteOutputTo(float[,] medium, int row)
    {
        for (int i = 0; i < this.outputCount; i++)
        {
            medium[row, i] = nodes[nodes.Length - outputCount + i - 1].Charge;
        }
    }
    internal void Decay(int time)
    {
        foreach(var node in this.nodes)
        {
            node.Decay(time);
        }
    }
}
