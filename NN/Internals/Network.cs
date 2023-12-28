namespace JBSnorro.NN.Internals;

internal sealed class Network : INetwork
{
    private readonly AxonType[] axonTypes;
    private readonly INeuronType[] nodeTypes;
    private readonly Neuron[] nodes;
    internal readonly Axon[] axons;  // excluding input axons
    private readonly int outputCount;

    public IReadOnlyList<Axon> Inputs { get; }
    public INetworkInitializer Initializer { get; }
    public float[] Output
    {
        get
        {
            float[] result = new float[outputCount];
            for (int i = 0; i < result.Length; i++)
                result[i] = nodes[nodes.Length - outputCount + i].Charge;
            return result;
        }
    }

    public Network(INeuronType[] nodeTypes,
                   int inputCount,
                   int outputCount,
                   AxonType?[,] connections,
                   INetworkInitializer initializer,
                   int? maxTime)
    {
        int nodeCount = nodeTypes.Length;
        Assert(connections.GetLength(0) == nodeCount);
        Assert(connections.GetLength(1) == nodeCount);
        Assert(inputCount <= nodeCount);
        Assert(outputCount <= nodeCount);
        Assert(initializer is not null);
        Assert(nodeTypes.All(t => t is not null));
        Assert(maxTime is null || maxTime.Value > 0);

        this.nodeTypes = nodeTypes;
        axonTypes = connections.Unique().Where(c => c != null).ToArray()!;
        nodes = new Neuron[nodeCount];
        var inputs = new Axon[inputCount];
        Inputs = inputs;
        this.outputCount = outputCount;

        int totalAxonCount = 0;
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
            nodes[i] = new Neuron(nodeTypes[i], axonCount);
            totalAxonCount += axonCount;
        }

        axons = new Axon[totalAxonCount];
        int axonsIndex = 0;
        for (int i = 0; i < nodeCount; i++)
        {
            for (int j = 0; j < nodeCount; j++)
            {
                var connectionType = connections[i, j];
                if (connectionType != null)
                {
                    var axon = new Axon(connectionType, nodes[j], connectionType.GetLength(i, j), connectionType.GetInitialWeight(i, j));
                    nodes[i].AddAxon(axon);
                    axons[axonsIndex++] = axon;
                }
            }
        }

        for (int i = 0; i < inputCount; i++)
        {
            inputs[i] = new Axon(AxonType.Input, nodes[i], length: Axon.InputLength, initialWeight: 1);
        }
        this.Initializer = initializer;
    }

    public void Initialize(IMachine machine)
    {
        this.Initializer.Activate(this.Inputs, machine);
    }
    public void Decay(int time)
    {
        foreach (var node in nodes)
        {
            node.Decay(time);
        }
    }
    public void Process(Feedback feedback, int time)
    {
        foreach (var axon in axons)
        {
            axon.ProcessFeedback(feedback, time);
        }
    }
}
