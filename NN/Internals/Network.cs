namespace JBSnorro.NN.Internals;
public interface INetworkFactory
{
    public static abstract (INeuronType[] nodeTypes, int inputCount, int outputCount, IAxonType?[,] connections, INetworkInitializer initializer) Create();
}
internal sealed class Network : INetwork
{
    private readonly IAxonType[] axonTypes;
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
                   IAxonType?[,] connections,
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
        this.axonTypes = connections.Unique().Where(c => c != null).ToArray()!;
        this.nodes = new Neuron[nodeCount];
        this.outputCount = outputCount;
        this.Inputs = new Axon[inputCount];

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
                var connectionType = connections[i, j];
                if (connectionType != null)
                {
                    var axon = new Axon(connectionType, nodes[j], connectionType.GetLength(i, j), connectionType.GetInitialWeight(i, j));
                    nodes[i].AddAxon(axon);
                    axons[axonsIndex++] = axon;
                }
            }
        }


        var inputs = (Axon[])this.Inputs;
        for (int i = 0; i < inputCount; i++)
        {
            inputs[i] = new Axon(IAxonType.Input, nodes[i], length: Axon.InputLength, initialWeight: 1);
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
