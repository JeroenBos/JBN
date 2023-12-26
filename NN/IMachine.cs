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

    float[,] Run(int maxTime);

    internal int Time { get; }

    /// <summary>
    /// Registers a <see cref="Neuron"/> that is potentially activated when this machine's time ticks.
    /// </summary>
    void RegisterPotentialActivation(Neuron neuron);
    void AddEmitAction(int time, Axon axon);
}
