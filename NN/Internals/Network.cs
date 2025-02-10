namespace JBSnorro.NN.Internals;

internal sealed class Network : INetwork
{
    private readonly IReadOnlyList<Neuron> neurons;
    private readonly UpdateWeightsDelegate update;
    private readonly float[] _output; // mutable

    /// <summary>
    /// Does not include input axons <see cref="Inputs"/> 
    /// </summary>
    public IReadOnlyList<Axon> Axons { get; }
    public IReadOnlyList<Axon> Inputs { get; }
    public IReadOnlyClock Clock { get; }
    public float[] Output
    {
        get
        {
            var output = this._output;
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = neurons[neurons.Count - output.Length + i].EffectiveCharge;
            }
            return output;
        }
    }

    public Network(IEnumerable<Either<INeuron, IAxonBuilder>> seeder,
                   int outputCount,
                   UpdateWeightsDelegate update,
                   IReadOnlyClock clock)
    {
        Assert(seeder is not null);
        Assert(update is not null);
        Assert(clock is not null);

        this.Clock = clock;
        this.update = update;
        var neurons = new List<Neuron>();
        this._output = new float[outputCount];
        List<Axon> axons = [];
        this.Axons = axons;

        var neuronsByLabel = new Dictionary<object, Neuron>();
        var inputs = new List<Axon>();
        foreach (var element in seeder)
        {
            if (element.TryGet(out INeuron _neuron))
            {
                var neuron = new Neuron(_neuron);
                neurons.Add(neuron);
                if (_neuron.Label is not null)
                {
                    neuronsByLabel[_neuron.Label] = neuron;
                }
            }
            else if (element.TryGet(out IAxonBuilder axonBuilder))
            {
                Neuron endpoint = axonBuilder.EndNeuronLabel is int index ? neurons[index] : neuronsByLabel[axonBuilder.EndNeuronLabel];

                if (ReferenceEquals(axonBuilder.StartNeuronLabel, IAxonBuilder.FromInputLabel))
                {
                    Axon axon = new Axon(axonBuilder, startpoint: null, endpoint);
                    inputs.Add(axon);
                }
                else
                {
                    Neuron startpoint = neuronsByLabel[axonBuilder.StartNeuronLabel];
                    Axon axon = new Axon(axonBuilder, startpoint, endpoint);
                    axons.Add(axon);
                    startpoint.AddAxon(axon);
                }
            }
            else throw new Exception("Invalid seed");
        }
        this.Inputs = inputs.ToArray();
        this.neurons = neurons.ToArray();

        if (outputCount > this.neurons.Count)
        {
            throw new Exception("outputCount > neurons.Count");
        }
    }

    public void Decay(IMachine machine)
    {
        foreach (var neuron in this.neurons)
        {
            neuron.Decay(this.Clock.Time);
            if (neuron.EffectiveCharge >= Neuron.threshold)
            {
                machine.RegisterPotentialExcitation(neuron);
            }
        }
    }
    public void Process(IFeedback feedback)
    {
        foreach (var axon in Axons)
        {
            axon.Process(feedback, this.update, this.Clock.Time);
        }
    }

    IReadOnlyList<Neuron> INetwork.Neurons => neurons;
}
