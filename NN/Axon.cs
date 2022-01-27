namespace JBSnorro.NN;

public sealed class Axon
{
    public static readonly int InputLength = 1; // if the machine starts at t=-1, this triggers them at t=0, and allows throwing when dt==0
    public Axon(AxonType type, Neuron endpoint, int length, float initialWeight = 1)
    {
        this.type = type;
        this.length = length;
        this.weight = initialWeight;
        this.endpoint = endpoint;
    }
    private readonly AxonType type;
    private readonly int length;
    private readonly Neuron endpoint;
    float weight;

    int timeOfDelivery = -1;
    internal void Activate(Machine machine)
    {
        this.timeOfDelivery = machine.Time + this.length;
        machine.AddEmitAction(this.timeOfDelivery, this);
    }
    internal void Emit(Machine machine)
    {
        this.endpoint.Receive(this.type, this.weight, machine);
    }
}
