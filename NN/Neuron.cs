namespace JBSnorro.NN;

public sealed class Neuron
{
    internal const float threshold = 1;
    public Neuron(NeuronType type, int axonCount)
    {
        this.type = type;
        this.axons = new Axon[axonCount];
        this.initializedAxonCount = 0;
    }
    internal void AddAxon(Axon axon)
    {
        this.axons[initializedAxonCount] = axon;
        this.initializedAxonCount++;
    }
    private int initializedAxonCount;
    readonly NeuronType type;
    readonly Axon[] axons;
    internal float Charge { get; private set; }

    private int decayUpdatedTime;
    private int lastActivatedTime = NeuronType.NEVER;
    private void Decay(Machine machine)
    {
        int t = machine.Time;
        for (; decayUpdatedTime < t; decayUpdatedTime++)
        {
            this.Charge *= this.type.GetDecay(t - decayUpdatedTime, t - lastActivatedTime);
        }
    }
    internal void Receive(AxonType axonType, float weight, Machine machine)
    {
        Decay(machine);
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
