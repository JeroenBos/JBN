using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

internal sealed class Neuron
{
    /// <summary>
    /// Certain operations are redundant if machine calls network.Decay every time step
    /// </summary>
    private static readonly bool MachineTriggersDecay = true;
    /// <summary>
    /// The threshold of charge over which a neuron fires.
    /// </summary>
    internal const float threshold = 1;

    private readonly INeuronType type;
    private readonly List<Axon> axons;
    /// <summary>
    /// The time up until and including which the decay has been updated. Decay happens at the start of a timestep.
    /// </summary>
    private int decayUpdatedTime;
    /// <summary>
    /// The time this neuron was activated last. Activation happens at the end of a timestep.
    /// </summary>
    private int lastActivatedTime = NEVER;
    /// <summary>
    /// The last timestep this neuron receipt charge.
    /// </summary>
    private int lastReceivedChargeTime = NEVER;
    internal float Charge { get; private set; }

    public Neuron(INeuronType type, float initialCharge = 0)
    {
        this.type = type;
        this.axons = new List<Axon>();
        this.Charge = initialCharge;
        if (initialCharge != 0)
        {
            this.lastReceivedChargeTime = 0;
            // this, in combination with decay being skipped at t=0, results in decay working as expected
        }
    }

    internal void AddAxon(Axon axon)
    {
        this.axons.Add(axon);
    }


    internal void Decay(int time)
    {
        for (; decayUpdatedTime <= time; decayUpdatedTime++)
        {
            this.Charge *= this.type.GetDecay(decayUpdatedTime - lastReceivedChargeTime, decayUpdatedTime - lastActivatedTime);
        }
    }
    internal void Receive(IAxonType axonType, float weight, IMachine machine)
    {
        // potentially, weight could be modified by the axon type
        Receive(weight, machine);
    }
    internal void Receive(float charge, IMachine machine)
    {
        if (!MachineTriggersDecay)
        {
            Decay(machine.Clock.Time);
        }
        bool alreadyRegistered = this.Charge >= threshold;
        this.Charge += charge;
        this.lastReceivedChargeTime = machine.Clock.Time;
        if (this.Charge >= threshold && !alreadyRegistered)
        {
            machine.RegisterPotentialActivation(this);
        }
    }
    internal void Activate(IMachine machine)
    {
        if (!MachineTriggersDecay && this.lastActivatedTime > this.decayUpdatedTime)
        {
            this.Decay(machine.Clock.Time);
        }

        this.lastActivatedTime = machine.Clock.Time;
        foreach (var axon in this.axons)
        {
            axon.Activate(machine.Clock.Time, machine);
        }
    }
}
