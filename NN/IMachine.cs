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

    void AddEmitAction(int time, Axon axon);
    void RegisterPotentialActivation(Neuron neuron);
}
