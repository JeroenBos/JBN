namespace JBSnorro.NN;

public class Network
{
    private readonly AxonType[] axonTypes;
    private readonly INeuronType[] nodeTypes;
    private readonly Neuron[] nodes;
    internal readonly Axon[] axons;  // excluding input axons
    private readonly int outputCount;

    public IReadOnlyList<Axon> Input { get; }
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

    public Network(INeuronType[] nodeTypes,
               int inputCount,
               int outputCount,
               AxonType?[,] connections)
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
        var input = new Axon[inputCount];
        this.Input = input;
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
            this.nodes[i] = new Neuron(nodeTypes[i], axonCount);
            totalAxonCount += axonCount;
        }

        this.axons = new Axon[totalAxonCount];
        int axonsIndex = 0;
        for (int i = 0; i < nodeCount; i++)
        {
            for (int j = 0; j < nodeCount; j++)
            {
                var connectionType = connections[i, j];
                if (connectionType != null)
                {
                    var axon = new Axon(connectionType, this.nodes[j], connectionType.GetLength(i, j), connectionType.GetInitialWeight(i, j));
                    this.nodes[i].AddAxon(axon);
                    axons[axonsIndex++] = axon;
                }
            }
        }

        for (int i = 0; i < inputCount; i++)
        {
            input[i] = new Axon(AxonType.Input, this.nodes[i], length: Axon.InputLength, initialWeight: 1);
        }
    }


    internal void Decay(int time)
    {
        foreach (var node in this.nodes)
        {
            node.Decay(time);
        }
    }
    internal void ProcessFeedback(float dopamine, float cortisol, int time)
    {
        foreach (var axon in this.axons)
        {
            axon.ProcessFeedback(dopamine, cortisol, time);
        }
    }
}
