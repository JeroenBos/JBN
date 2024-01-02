namespace JBSnorro.NN.Internals;
internal sealed class Network : INetwork
{
    private readonly IReadOnlyList<INeuronType> neuronTypes;
    private readonly Neuron[] neurons;
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
                result[i] = neurons[neurons.Length - outputCount + i].Charge;
            return result;
        }
    }

    public Network(IReadOnlyList<INeuronType> neuronTypes,
                   int inputCount,
                   int outputCount,
                   GetAxonConnectionDelegate getConnection,
                   IReadOnlyClock clock)
    {
        Assert(neuronTypes is not null);
        Assert(neuronTypes.All(type => type is not null));
        Assert(inputCount <= neuronTypes.Count);
        Assert(outputCount <= neuronTypes.Count);
        Assert(getConnection is not null);

        this.Clock = clock;
        this.neuronTypes = neuronTypes;
        this.neurons = neuronTypes.Select(type => new Neuron(type, 0)).ToArray();
        this.Inputs = new Axon[inputCount];
        this.outputCount = outputCount;
        this.Axons = new List<Axon>();

        var axons = (List<Axon>)this.Axons;
        for (int i = 0; i < neuronTypes.Count; i++)
        {
            for (int j = 0; j < neuronTypes.Count; j++)
            {
                var axonInitialization = getConnection(i, j);
                if (axonInitialization != null)
                {
                    var axon = new Axon(axonInitialization.AxonType, neurons[j], axonInitialization.Length, axonInitialization.InitialWeight);
                    neurons[i].AddAxon(axon);
                    axons.Add(axon);
                }
            }
        }

        var inputs = (Axon[])this.Inputs;
        for (int i = 0; i < inputCount; i++)
        {
            inputs[i] = new Axon(IAxonType.Input, this.neurons[i], IAxonInitialization.Input.Length, IAxonInitialization.Input.InitialWeight);
        }
    }

    public void Decay()
    {
        foreach (var neuron in this.neurons)
        {
            neuron.Decay(this.Clock.Time);
        }
    }
    public void Process(Feedback feedback)
    {
        foreach (var axon in Axons)
        {
            axon.ProcessFeedback(feedback, this.Clock.Time);
        }
    }
}

internal delegate IAxonInitialization? GetAxonConnectionDelegate(int neuronFromIndex, int neuronToIndex);