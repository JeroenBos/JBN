namespace JBSnorro.NN.Internals;

[DebuggerDisplay("Neuron({index == null ? -1 : index}, Charge={Charge})")]
internal sealed class Neuron
{
    /// <summary>
    /// The threshold of charge over which a neuron fires.
    /// </summary>
    internal const float threshold = 1;

    private readonly INeuronType type;

    /// <summary>
    /// Note on terminology:
    /// In biology, a neuron has only one axon. It branches at the tip, and has many terminals connecting to other neurons' dendrites at synapses.
    /// At a synapse, positive and negative charges accumulate and slowly depolarize the dendrite. Only sufficient depolarization reaches the neuron, let's say.
    /// That reflects more the current implementation of the neuron: charges accumulate and at a threshold it fires.
    /// The dendritic process is well-modeled here (accumulation until threshold, like digital: 0 or 1), but the synaptic isn't modeled well: accumulate and pass, like analog.
    /// 
    /// There are multiple kinds of neurotransmitters which have different polarizing, spatial and temporal effects on the synapse. That I sort of model with decay
    /// and with the neuronal charge being multidimensional.
    /// I'm debating whether my implementation is equivalent: it's as though the axons connect directly to the neurons, without dendrites.
    /// Still enough charge has to accumulate to excite it, and they are accumulated anyway, just in a different way. The question is whether
    /// synapse charge accumulation, dendritic charge propagation, neuronal charge accumulation commute.
    /// If any of them do, given that accumulation commutes, then I can swap dedritic charge propagation with synapse charge accumulation, and then say that 
    /// the axons in my implementation "also" implement contain the dendrites. It seems equivalent to me. Instead of accumulating accumulations, we just accumulate all:
    /// whether it reaches a threshold won't be affected, I think.
    /// </summary>
    private readonly List<Axon> axons;
    /// <summary>
    /// The time up until and including which the decay has been updated. Decay happens at the end of a timestep.
    /// </summary>
    private int decayUpdatedTime = -1;
    /// <summary>
    /// The time this neuron was excited last. Excitation happens at the end of a timestep.
    /// </summary>
    private int lastExcitationTime = NEVER;
    /// <summary>
    /// The last timestep this neuron receipt charge.
    /// </summary>
    private int lastReceivedChargeTime = NEVER;
    internal float Charge { get; private set; }

#if DEBUG
    private readonly int? index;
#endif
    public Neuron(INeuronType type, float initialCharge = 0, int? index = null)
    {
        this.type = type;
        this.axons = [];
        this.Charge = initialCharge;
#if DEBUG
        this.index = index;
#endif
        if (initialCharge != 0)
        {
            this.lastReceivedChargeTime = 0;
        }
    }

    internal void AddAxon(Axon axon)
    {
        this.axons.Add(axon);
    }


    internal void Decay(int time)
    {
        // Decay is at the end of a time step, and so we decay the current time also. The neuron's excitation, if any, has been elicited already.
        // That means that if a neuron got charge this step, the type.GetDecay(..) is called with timeSinceLastChargeReceipt: 0
        // +1 because the decayUpdatedTime has already been updated
        for (int decayUpdatingTime = this.decayUpdatedTime + 1; decayUpdatingTime <= time; decayUpdatingTime++)
        {
            this.Charge *= this.type.GetDecay(decayUpdatingTime - lastReceivedChargeTime, decayUpdatingTime - lastExcitationTime);
        }
        this.decayUpdatedTime = time;
    }
    internal void Receive(float charge, IMachine machine)
    {
        bool alreadyRegistered = this.Charge >= threshold;
        this.Charge += charge;
        this.lastReceivedChargeTime = machine.Clock.Time;
        if (this.Charge >= threshold && !alreadyRegistered)
        {
            machine.RegisterPotentialExcitation(this);
        }
    }
    /// <returns>whether the this was the first time this neuron was excited this timestep. </returns>
    internal bool Excite(Machine machine)
    {
        if (this.lastExcitationTime == machine.Clock.Time)
            return false;

        this.lastExcitationTime = machine.Clock.Time;
        foreach (var axon in this.axons)
        {
            axon.Excite(machine);
        }
        return true;
    }
}
