namespace JBSnorro.NN.Internals;
internal sealed class Network : INetwork
{
    private readonly IReadOnlyList<INeuronType> neuronTypes;
    private readonly Neuron[] neurons;
    private readonly float[] _output;

    public IReadOnlyList<Axon> Axons { get; } // excluding input axons
    public IReadOnlyList<Axon> Inputs { get; }
    public IReadOnlyClock Clock { get; }
    public float[] Output
    {
        get
        {
            var output = this._output;
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = neurons[neurons.Length - output.Length + i].Charge;
            }
            return output;
        }
    }

    /// <param name="neuronTypes">One type per neuron.</param>
    public Network(IReadOnlyList<INeuronType> neuronTypes,
                   int outputCount,
                   GetAxonConnectionDelegate getConnection,
                   IReadOnlyClock clock)
    {
        int neuronCount = neuronTypes.Count;
        Assert(neuronTypes is not null);
        Assert(neuronTypes.All(type => type is not null));
        Assert(outputCount <= neuronCount);
        Assert(getConnection is not null);

        this.Clock = clock;
        this.neuronTypes = neuronTypes;
        this.neurons = neuronTypes.Select((type, index) => new Neuron(type, 0, index)).ToArray();
        this._output = new float[outputCount];
        this.Axons = new List<Axon>();

        var axons = (List<Axon>)this.Axons;
        for (int i = 0; i < neuronCount; i++)
        {
            for (int j = 0; j < neuronCount; j++)
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

        var inputs = new List<Axon>();
        for (int i = 0; i < neuronCount; i++)
        {
            var axonInitialization = getConnection(-1, i);
            if (axonInitialization != null)
            {
                inputs.Add(new Axon(IAxonType.Input, this.neurons[i], axonInitialization.Length, axonInitialization.InitialWeight));
            }
        }
        this.Inputs = inputs.ToArray();
    }

    public void Decay()
    {
        foreach (var neuron in this.neurons)
        {
            neuron.Decay(this.Clock.Time);
        }
    }
    public void Process(IFeedback feedback)
    {
        foreach (var axon in Axons)
        {
            axon.Process(feedback, this.Clock.Time);
        }
    }
}

internal delegate IAxonInitialization? GetAxonConnectionDelegate(int neuronFromIndex, int neuronToIndex);