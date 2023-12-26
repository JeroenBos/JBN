namespace JBSnorro.NN;

public sealed class Axon
{
    public static readonly int InputLength = 1; // if the machine starts at t=-1, this triggers them at t=0, and allows throwing when dt==0
    public Axon(AxonType type, Neuron endpoint, int length, float initialWeight)
    {
        if (length <= 0 || length > MAX_LENGTH)
            throw new ArgumentOutOfRangeException(nameof(length));

        this.type = type;
        this.length = length;
        this.weight = initialWeight;
        this.endpoint = endpoint;
    }

    private readonly AxonType type;
    private readonly int length;
    private readonly Neuron endpoint;
    private float weight;
    private int timeOfDelivery = NEVER;
    private int activationCount = 0;
    private float averageTimeBetweenActivations = float.NaN;
    internal float Weight => weight;
    internal void Activate(Machine machine)
    {
        this.activationCount++;

        int newTimeOfDelivery = machine.Time + this.length;
        int timeBetweenActivations = newTimeOfDelivery - this.timeOfDelivery;
        this.timeOfDelivery = newTimeOfDelivery;
        if (float.IsNaN(averageTimeBetweenActivations))
        {
            averageTimeBetweenActivations = machine.Time;
        }
        else
        {
            averageTimeBetweenActivations = (averageTimeBetweenActivations * (activationCount - 1) + timeBetweenActivations) / activationCount;
        }
        machine.AddEmitAction(this.timeOfDelivery, this);
    }
    internal void Emit(Machine machine)
    {
        this.endpoint.Receive(this.type, this.weight, machine);
        // leave timeOfDelivery for feedback
    }
    internal void ProcessFeedback(float dopamine, float cortisol, int time)
    {
        int timeSinceLastActivation = time - this.timeOfDelivery - this.length;
        // TODO: pass along a vector representing position
        this.weight = this.type.GetUpdatedWeight(this.weight,
                                                 timeSinceLastActivation,
                                                 this.averageTimeBetweenActivations,
                                                 this.activationCount,
                                                 dopamine,
                                                 cortisol);
    }
}
