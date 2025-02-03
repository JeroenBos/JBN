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

    public Network(IEnumerable<Either<INeuronType, IAxonBuilder>> seeder,
                   int outputCount,
                   IReadOnlyClock clock)
    {
        Assert(seeder is not null);
        Assert(clock is not null);

        this.Clock = clock;
        var neurons = new List<Neuron>();
        this._output = new float[outputCount];
        List<Axon> axons = [];
        this.Axons = axons;
        
        var inputs = new List<Axon>();
        foreach (var element in seeder)
        {
            if (element.TryGet(out INeuronType type))
            {
                neurons.Add(new Neuron(type, type.InitialCharge, neurons.Count));
            }
            else if (element.TryGet(out IAxonBuilder axonBuilder))
            {
                Neuron endpoint = neurons[axonBuilder.EndNeuronIndex];
                Axon axon = new Axon(axonBuilder, endpoint);

                if (axonBuilder.StartNeuronIndex == IAxonBuilder.FROM_INPUT)
                {
                    inputs.Add(axon);
                }
                else
                {
                    axons.Add(axon);
                    Neuron startpoint = neurons[axonBuilder.StartNeuronIndex];
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
