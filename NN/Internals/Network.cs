namespace JBSnorro.NN.Internals;
internal sealed class Network : INetwork
{
    private readonly IReadOnlyList<INeuronType> nodeTypes;
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

    public Network(IReadOnlyList<INeuronType> nodeTypes,
                   int inputCount,
                   int outputCount,
                   GetAxonConnectionDelegate getConnection,
                   IReadOnlyClock clock)
    {
        Assert(nodeTypes is not null);
        Assert(nodeTypes.All(type => type is not null));
        Assert(inputCount <= nodeTypes.Count);
        Assert(outputCount <= nodeTypes.Count);
        Assert(getConnection is not null);

        this.Clock = clock;
        this.nodeTypes = nodeTypes;
        this.nodes = nodeTypes.Select(type => new Neuron(type, 0)).ToArray();
        this.Inputs = new Axon[inputCount];
        this.outputCount = outputCount;
        this.Axons = new List<Axon>();

        var axons = (List<Axon>)this.Axons;
        for (int i = 0; i < nodeTypes.Count; i++)
        {
            for (int j = 0; j < nodeTypes.Count; j++)
            {
                var axonInitialization = getConnection(i, j);
                if (axonInitialization != null)
                {
                    var axon = new Axon(axonInitialization.AxonType, nodes[j], axonInitialization.Length, axonInitialization.InitialWeight);
                    nodes[i].AddAxon(axon);
                    axons.Add(axon);
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

internal delegate IAxonInitialization? GetAxonConnectionDelegate(int neuronFromIndex, int neuronToIndex);