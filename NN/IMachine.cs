using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface IMachine
{
    public static IMachine Create(Network network)
    {
        return new Machine(network);
    }
    public static IMachine Create(Network network, GetFeedbackDelegate getFeedback)
    {
        return new Machine(network, getFeedback);
    }

    public float[,] Run(int maxTime);


    /// <summary>
    /// Registers a <see cref="Neuron"/> that is potentially activated when this machine's time ticks.
    /// </summary>
    public void RegisterPotentialActivation(Neuron neuron);
    /// <summary>
    /// Sets the specified axon to emit charge at the specified time.
    /// </summary>
    public void AddEmitAction(int time, Axon axon);

    public event OnTickDelegate OnTick;
    
    internal int Time { get; }
}
