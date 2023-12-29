namespace JBSnorro.NN.Internals;
public interface INetworkFactory
{
    public static abstract (INeuronType[] nodeTypes, int inputCount, int outputCount, IAxonInitialization?[,] connections, INetworkInitializer initializer) Create();
}
internal sealed class Network : INetwork
{
    private readonly INeuronType[] nodeTypes;
    private readonly Neuron[] nodes;
    private readonly int outputCount;

    public IReadOnlyList<Axon> Axons { get; } // excluding input axons
    public IReadOnlyList<Axon> Inputs { get; }
    public IReadOnlyClock Clock { get; }
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
                   IAxonInitialization?[,] connections,
                   IReadOnlyClock clock)
    {
        Assert(nodeTypes is not null);
        int nodeCount = nodeTypes.Length;
        Assert(connections.GetLength(0) == nodeCount);
        Assert(connections.GetLength(1) == nodeCount);
        Assert(inputCount <= nodeCount);
        Assert(outputCount <= nodeCount);
        Assert(nodeTypes.All(type => type is not null));

        this.Clock = clock;
        this.nodeTypes = nodeTypes;
        this.nodes = new Neuron[nodeCount];
        this.Inputs = new Axon[inputCount];
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

        var axons = new Axon[totalAxonCount];
        this.Axons = axons;
        int axonsIndex = 0;
        for (int i = 0; i < nodeCount; i++)
        {
            for (int j = 0; j < nodeCount; j++)
            {
                var axonInitialization = connections[i, j];
                if (axonInitialization != null)
                {
                    var axon = new Axon(axonInitialization.AxonType, nodes[j], axonInitialization.Length, axonInitialization.InitialWeight);
                    nodes[i].AddAxon(axon);
                    axons[axonsIndex++] = axon;
                }
            }
        }


        var inputs = (Axon[])this.Inputs;
        for (int i = 0; i < inputCount; i++)
        {
            inputs[i] = new Axon(IAxonType.Input, nodes[i], IAxonInitialization.Input.Length, IAxonInitialization.Input.InitialWeight);
        }
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
        foreach (var axon in Axons)
        {
            axon.ProcessFeedback(feedback, time);
        }
    }
}
