using JBSnorro.NN;
namespace JBSnorro.NN.Internals;

internal sealed class Axon
{
    public static readonly int InputLength = 1; // if the machine starts at t=-1, this triggers them at t=0, and allows throwing when dt==0
    public Axon(AxonType type, Neuron endpoint, int length, float initialWeight)
    {
        if (length <= 0 || length > MAX_LENGTH)
            throw new ArgumentOutOfRangeException(nameof(length));

        this.type = type;
        this.length = length;
        weight = initialWeight;
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
    internal void Activate(int time, IMachine machine)
    {
        activationCount++;

        int newTimeOfDelivery = time + length;
        int timeBetweenActivations = newTimeOfDelivery - timeOfDelivery;
        timeOfDelivery = newTimeOfDelivery;
        if (float.IsNaN(averageTimeBetweenActivations))
        {
            averageTimeBetweenActivations = time;
        }
        else
        {
            averageTimeBetweenActivations = (averageTimeBetweenActivations * (activationCount - 1) + timeBetweenActivations) / activationCount;
        }
        machine.AddEmitAction(timeOfDelivery, this);
    }
    internal void Emit(Machine machine)
    {
        endpoint.Receive(type, weight, machine);
        // leave timeOfDelivery for feedback
    }
    internal void ProcessFeedback(Feedback feedback, int time)
    {
        int timeSinceLastActivation = time - timeOfDelivery - length;
        // TODO: pass along a vector representing position
        weight = type.GetUpdatedWeight(weight,
                                                 timeSinceLastActivation,
                                                 averageTimeBetweenActivations,
                                                 activationCount,
                                                 feedback);
    }
}

