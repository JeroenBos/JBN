using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface IMachine
{
    public static IMachine Create(INetwork network)
    {
        return new Machine(network);
    }
    public static IMachine Create(INetwork network, GetFeedbackDelegate getFeedback)
    {
        return new Machine(network, getFeedback);
    }

    public event OnTickDelegate OnTick;
    public float[,] Run(int maxTime);

    /// <summary>
    /// Registers a <see cref="Neuron"/> that is potentially activated when this machine's time ticks.`
    /// </summary>
    internal void RegisterPotentialActivation(Neuron neuron);
    /// <summary>
    /// Sets the specified axon to emit charge at the specified time.
    /// </summary>
    /// <param name="timeOfDelivery">The time the emit is to be delivered at the end of its axon. </param>
    internal void AddEmitAction(int timeOfDelivery, Axon axon);

    internal int Time { get; }
}

public interface IMachine<TNetworkFactory> where TNetworkFactory : INetworkFactory
{
    public static IMachine Create(int? maxTime)
    {
        var clock = IClock.Create(maxTime);
        var (nodeTypes, inputCount, outputCount, connections, initializer) = TNetworkFactory.Create();
        var network = INetwork.Create(nodeTypes, inputCount, outputCount, connections, initializer, clock);
        var machine = IMachine.Create(network);
        initializer.Activate(network.Axons, machine);
        return machine;
    }
}