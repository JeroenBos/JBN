namespace JBSnorro.NN.Internals;

internal sealed class Network : INetwork
{
    private readonly IReadOnlyList<Neuron> neurons;
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
                   IEqualityComparer<object?>? labelEqualityComparer,
                   IReadOnlyClock clock)
    {
        Assert(seeder is not null);
        Assert(clock is not null);

        this.Clock = clock;
        var neurons = new List<Neuron>();
        this._output = new float[outputCount];
        List<Axon> axons = [];
        this.Axons = axons;

        var neuronsByLabel = new Dictionary<object, Neuron>(labelEqualityComparer);
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
                Axon axon = new Axon(axonBuilder, endpoint);

                if (ReferenceEquals(axonBuilder.StartNeuronLabel, IAxonBuilder.FromInputLabel))
                {
                    inputs.Add(axon);
                }
                else
                {
                    axons.Add(axon);
                    Neuron startpoint = neuronsByLabel[axonBuilder.StartNeuronLabel];
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
            axon.Process(feedback, this.Clock.Time);
        }
    }

    IReadOnlyList<Neuron> INetwork.Neurons => neurons;
}
