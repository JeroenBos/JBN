namespace JBSnorro.NN.Internals;

[DebuggerDisplay("Axon(→{endpoint.index}, type={type}, w={weights[0]})")]
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
    /// <summary>
    /// Weights represent charges delivered to a neuron on emission.
    /// The weights are simply added to the charges (implemented in <see cref="Neuron.Receive"/>).
    /// </summary>
    private readonly float[] weights;
    private int timeOfDelivery = NEVER;
    private int excitationCount = 0;
    /// <summary>
    /// Average time between excitations. NaN means hasn't fired yet.
    /// </summary>
    private float averageTimeBetweenExcitations = float.NaN;
    /// <returns>the time of delivery.</returns>
    internal void Excite(Machine machine)
    {
        excitationCount++;

        int newTimeOfDelivery = machine.Clock.Time + this.length;
        int timeBetweenExcitations = newTimeOfDelivery - this.timeOfDelivery;
        this.timeOfDelivery = newTimeOfDelivery;
        if (float.IsNaN(averageTimeBetweenExcitations))
        {
            averageTimeBetweenExcitations = machine.Clock.Time + 1;
        }
        else
        {
            averageTimeBetweenExcitations = (averageTimeBetweenExcitations * (excitationCount - 1) + timeBetweenExcitations) / excitationCount;
        }
        machine.AddEmitAction(timeOfDelivery, this);
    }
    internal void Emit(IMachine machine)
    {
        endpoint.Receive(weights, machine);
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

