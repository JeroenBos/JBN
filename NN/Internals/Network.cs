namespace JBSnorro.NN.Internals;

internal sealed class Network : INetwork
{
    private readonly List<Neuron> neurons;
    private readonly List<Axon> axons;
    private readonly UpdateWeightsDelegate update;
    private readonly Dictionary<object, Neuron> neuronsByLabel;
    private readonly float[] _output; // mutable

    public IReadOnlyList<INeuron> Neurons => neurons;
    /// <summary>
    /// Does not include input axons <see cref="Inputs"/> 
    /// </summary>
    public IReadOnlyList<Axon> Axons => axons;
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

    public Network(IEnumerable<Either<INeuron, IAxonBuilder>> elements,
                   int outputCount,
                   UpdateWeightsDelegate update,
                   IReadOnlyClock clock)
    {
        Assert(elements is not null);
        Assert(clock is not null);

        this.Clock = clock;
        this.neurons = new();
        this.axons = new();
        this.update = update;
        this.neuronsByLabel = new();
        this._output = new float[outputCount];
        this.Inputs = this.AddInternal(elements)?.ToArray() ?? throw new ContractException($"{nameof(this.Add)} must return a list when called from the ctor");

        if (outputCount > this.neurons.Count)
        {
            throw new ContractException($"{nameof(outputCount)} > neurons.Count");
        }
    }

    private List<Axon>? AddInternal(IEnumerable<Either<INeuron, IAxonBuilder>> elements)
    {
        var inputs = this.Inputs == null ? new List<Axon>() : null;
        foreach (var element in elements)
        {
            if (element.Get(out INeuron _neuron, out IAxonBuilder axonBuilder))
            {
                var neuron = new Neuron(_neuron);
                neurons.Add(neuron);
                if (_neuron.Label is not null)
                {
                    neuronsByLabel[_neuron.Label] = neuron;
                }
            }
            else
            {
                Neuron endpoint = axonBuilder.EndNeuronLabel is int index ? neurons[index] : neuronsByLabel[axonBuilder.EndNeuronLabel];

                if (ReferenceEquals(axonBuilder.StartNeuronLabel, IAxonBuilder.FromInputLabel))
                {
                    if (inputs is null)
                    {
                        throw new InvalidOperationException("Input axons can only be added at the initialization of the network");
                    }
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
        }

        return inputs;
    }
    public void Add(IEnumerable<Either<INeuron, IAxonBuilder>> elements)
    {
        AddInternal(elements);
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

    IReadOnlyList<Neuron> INetwork._Neurons => neurons;
}
