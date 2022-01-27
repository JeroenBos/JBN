namespace JBSnorro.NN;

public sealed class Neuron
{
    internal const float threshold = 1;
    public Neuron(INeuronType type, int axonCount, float initialCharge = 0)
    {
        this.type = type;
        this.axons = new Axon[axonCount];
        this.initializedAxonCount = 0;
        this.Charge = initialCharge;
    }
    internal void AddAxon(Axon axon)
    {
        this.axons[initializedAxonCount] = axon;
        this.initializedAxonCount++;
    }
    private int initializedAxonCount;
    readonly INeuronType type;
    readonly Axon[] axons;
    internal float Charge { get; private set; }

    /// <summary> The time up until and including which the decay has been updated. Decay happens at the start of a timestep. </summary>
    private int decayUpdatedTime;
    /// <summary> The time this neuron was activated last. Activation happens at the end of a timestep. </summary>
    private int lastActivatedTime = INeuronType.NEVER;
    internal void Decay(int time)
    {
        if (time == 0)
            return; // there's no decay at time 0
        for (; decayUpdatedTime <= time; decayUpdatedTime++)
        {
            // decay gets a +1 because it's at the start of the time, whereas activation is at the end. time is in the middle
            this.Charge *= this.type.GetDecay(time - decayUpdatedTime + 1, time - lastActivatedTime);
        }
    }
    internal void Receive(AxonType axonType, float weight, Machine machine)
    {
        // Decay(machine.Time); redundant if machine calls network.Decay every time step
        bool alreadyRegistered = this.Charge >= threshold;
        this.Charge += weight;
        if (this.Charge >= threshold && !alreadyRegistered)
        {
            machine.RegisterPotentialActivation(this);
        }
    }
    internal void Activate(Machine machine)
    {
        lastActivatedTime = machine.Time;
        foreach (var axon in this.axons)
        {
            axon.Activate(machine);
        }
    }
}
