namespace JBSnorro.NN.Internals;

internal sealed class Network : INetwork
{
    private readonly IReadOnlyList<INeuronType> neuronTypes;
    private readonly IReadOnlyList<Neuron> neurons;
    private readonly float[] _output; // mutable
    private UpdateWeightsDelegate updateWeights;

    /// <summary>
    /// Does not include input axons <see cref="Inputs"/> 
    /// </summary>
    public IReadOnlyList<Axon> Axons { get; }
    public IReadOnlyList<Axon> Inputs { get; }
    public int ChargeCount { get; }
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

    /// <param name="neuronTypes">One type per neuron.</param>
    public Network(IReadOnlyList<INeuronType> neuronTypes,
                   int outputCount,
                   GetAxonConnectionDelegate getConnection,
                   UpdateWeightsDelegate updateWeights,
                   IReadOnlyClock clock)
    {
        int neuronCount = neuronTypes.Count;
        Assert(neuronTypes is not null);
        Assert(neuronTypes.All(type => type is not null));
        Assert(outputCount <= neuronCount);
        Assert(getConnection is not null);

        this.Clock = clock;
        this.neuronTypes = neuronTypes;
        this.neurons = neuronTypes.Select((type, index) => new Neuron(type, type.InitialCharge, index)).ToArray();
        this._output = new float[outputCount];
        this.updateWeights = updateWeights;
        List<Axon> axons = [];
        this.Axons = axons;
        int chargeCount = neurons[0].Charges.Count;
        
        for (int i = 0; i < neuronCount; i++)
        {
            for (int j = 0; j < neuronCount; j++)
            {
                var axonLength = getConnection(i, j);
                if (axonLength != null)
                {
                    var axon = new Axon(axonLength.Length, neurons[j], this.ChargeCount, this.updateWeights);
                    neurons[i].AddAxon(axon);
                    axons.Add(axon);
                }
            }
        }

        var inputs = new List<Axon>();
        for (int i = 0; i < neuronCount; i++)
        {
            var axonLength = getConnection(INetwork.FROM_INPUT, i);
            if (axonLength != null)
            {
                inputs.Add(new Axon(axonLength.Length, this.neurons[i], this.ChargeCount, this.updateWeights));
            }
        }
        this.Inputs = inputs.ToArray();
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
            axon.Process(feedback, this.Clock.Time, this.updateWeights);
        }
    }

    IReadOnlyList<Neuron> INetwork.Neurons => neurons;
}
