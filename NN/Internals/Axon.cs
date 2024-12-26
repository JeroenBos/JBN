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
    private int excitationCount = 0;
    /// <summary>
    /// Average time between excitations. NaN means hasn't fired yet.
    /// </summary>
    private float averageTimeBetweenExcitations = float.NaN;
    /// <returns>the time of delivery.</returns>
    internal int Excite(int currentTime)
    {
        excitationCount++;

        int newTimeOfDelivery = currentTime + this.length;
        int timeBetweenExcitations = newTimeOfDelivery - this.timeOfDelivery;
        this.timeOfDelivery = newTimeOfDelivery;
        if (float.IsNaN(averageTimeBetweenExcitations))
        {
            averageTimeBetweenExcitations = currentTime + 1;
        }
        else
        {
            averageTimeBetweenExcitations = (averageTimeBetweenExcitations * (excitationCount - 1) + timeBetweenExcitations) / excitationCount;
        }
        return this.timeOfDelivery;
    }
    internal void Emit(Machine machine)
    {
        endpoint.Receive(type.GetCharge(weights), machine);
    }
    internal void Process(IFeedback feedback, int time)
    {
        int timeSinceLastExcitation = time - timeOfDelivery - length;
        // TODO: pass along a vector representing position
        type.UpdateWeights(weights,
                           timeSinceLastExcitation,
                           averageTimeBetweenExcitations,
                           excitationCount,
                           feedback);
    }
}

