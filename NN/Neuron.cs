using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public sealed class Neuron
{
    private static readonly bool MachineTriggersDecay = true;  // certain operations are redundant if machine calls network.Decay every time step

    internal const float threshold = 1;
    private readonly INeuronType type;
    private readonly Axon[] axons;
    private int initializedAxonCount;
    /// <summary> The time up until and including which the decay has been updated. Decay happens at the start of a timestep. </summary>
    private int decayUpdatedTime;
    /// <summary> The time this neuron was activated last. Activation happens at the end of a timestep. </summary>
    private int lastActivatedTime = NEVER;
    private int lastReceivedChargeTime = NEVER;
    internal float Charge { get; private set; }

    public Neuron(INeuronType type, int axonCount, float initialCharge = 0)
    {
        this.type = type;
        this.axons = new Axon[axonCount];
        this.initializedAxonCount = 0;
        this.Charge = initialCharge;
        if (initialCharge != 0)
        {
            this.lastReceivedChargeTime = 0;
            // this, in combination with decay being skipped at t=0, results in decay working as expected
        }
    }

    internal void AddAxon(Axon axon)
    {
        this.axons[initializedAxonCount] = axon;
        this.initializedAxonCount++;
    }


    internal void Decay(int time)
    {
        for (; decayUpdatedTime <= time; decayUpdatedTime++)
        {
            this.Charge *= this.type.GetDecay(decayUpdatedTime - lastReceivedChargeTime, decayUpdatedTime - lastActivatedTime);
        }
    }
    internal void Receive(AxonType axonType, float weight, Machine machine)
    {
        // potentially, weight could be modified by the axon type
        Receive(weight, machine);
    }
    internal void Receive(float charge, Machine machine)
    {
        if (!MachineTriggersDecay)
        {
            Decay(machine.Time);
        }
        bool alreadyRegistered = this.Charge >= threshold;
        this.Charge += charge;
        this.lastReceivedChargeTime = machine.Time;
        if (this.Charge >= threshold && !alreadyRegistered)
        {
            machine.RegisterPotentialActivation(this);
        }
    }
    internal void Activate(Machine machine)
    {
        if (!MachineTriggersDecay && this.lastActivatedTime > this.decayUpdatedTime)
        {
            this.Decay(machine.Time);
        }

        this.lastActivatedTime = machine.Time;
        foreach (var axon in this.axons)
        {
            axon.Activate(machine.Time, machine);
        }
    }
}
