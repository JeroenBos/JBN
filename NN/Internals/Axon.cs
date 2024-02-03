using System.Diagnostics;

namespace JBSnorro.NN.Internals;

[DebuggerDisplay("Axon(→{endpoint.index})")]
internal sealed class Axon
{
    public static readonly int InputLength = 1; // if the machine starts at t=-1, this delivers initial inputs at t=0, allowing throwing when dt==0
    public Axon(IAxonType type, Neuron endpoint)
    {
        if (type.Length <= 0 || type.Length > MAX_AXON_LENGTH)
            throw new ArgumentOutOfRangeException($"{nameof(type)}.{nameof(IAxonType.Length)}");

        this.type = type;
        this.length = type.Length;
        this.weights = type.InitialWeights.ToArray();
        this.endpoint = endpoint;
    }

    private readonly IAxonType type;
    private readonly int length;
    private readonly Neuron endpoint;
    private readonly float[] weights;
    private int timeOfDelivery = NEVER;
    private int activationCount = 0;
    private float averageTimeBetweenActivations = float.NaN;
    /// <returns>the time of delivery.</returns>
    internal int Excite(int currentTime)
    {
        activationCount++;

        int newTimeOfDelivery = currentTime + this.length;
        int timeBetweenActivations = newTimeOfDelivery - this.timeOfDelivery;
        this.timeOfDelivery = newTimeOfDelivery;
        if (float.IsNaN(averageTimeBetweenActivations))
        {
            averageTimeBetweenActivations = currentTime + 1;
        }
        else
        {
            averageTimeBetweenActivations = (averageTimeBetweenActivations * (activationCount - 1) + timeBetweenActivations) / activationCount;
        }
        return this.timeOfDelivery;
    }
    internal void Emit(Machine machine)
    {
        endpoint.Receive(type.GetCharge(weights), machine);
    }
    internal void Process(IFeedback feedback, int time)
    {
        int timeSinceLastActivation = time - timeOfDelivery - length;
        // TODO: pass along a vector representing position
        type.UpdateWeights(weights,
                           timeSinceLastActivation,
                           averageTimeBetweenActivations,
                           activationCount,
                           feedback);
    }
}

