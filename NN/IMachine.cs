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

    public event OnTickDelegate OnTicked;
    public float[] Run(int maxTime);
    public IReadOnlyList<float[]> RunCollect(int maxTime)
    {
        var result = new List<float[]>(capacity: maxTime);
        this.OnTicked += OnTicked;

        try
        {
            Run(maxTime);
        }
        finally
        {
            this.OnTicked -= OnTicked;
        }
        return result;


        void OnTicked(IMachine sender, OnTickEventArgs e)
        {
            result.Add(e.Output.ToArray());
        }
    }

    public IReadOnlyClock Clock { get; }

    /// <summary>
    /// Gets the current charges of the output neurons of the network.
    /// </summary>
    internal float[] Output { get; }
    /// <summary>
    /// Registers a <see cref="Neuron"/> that is potentially activated when this machine's time ticks.`
    /// </summary>
    internal void RegisterPotentialActivation(Neuron neuron);
    /// <summary>
    /// Sets the specified axon to emit charge at the specified time.
    /// </summary>
    /// <param name="timeOfDelivery">The time the emit is to be delivered at the end of its axon. </param>
    internal void AddEmitAction(int timeOfDelivery, Axon axon);
}
