using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface IMachine
{
    public static IMachine Create(INetwork network, GetFeedbackDelegate getFeedback)
    {
        return new Machine(network, getFeedback);
    }

    public event OnTickDelegate OnTicked;
    /// <summary>
    /// Runs the network for the specified number of steps (or until the clock's max time) and returns the chargers the output neurons have at the end.
    /// </summary>
    /// <param name="maxTime">If <see langword="null" /> is specified, it runs until the clock's max time. </param>
    public float[] Run(int? maxTime = null);
    /// <summary>
    /// Calls <see cref="Run(int)"/> and collects all the outputs for each timestep in a list.
    /// </summary>
    /// <returns>the list of collected ouputs.</returns>
    public IReadOnlyList<float[]> RunCollect(int? maxTime = null)
    {
        List<float[]> result = maxTime is null ? new() : new(capacity: maxTime.Value);
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
            result.Add([..e.Output]);
        }
    }

    public IReadOnlyClock Clock { get; }

    /// <summary>
    /// Gets the current charges of the output neurons of the network.
    /// </summary>
    internal float[] Output { get; }
    /// <summary>
    /// Registers a <see cref="Neuron"/> that is potentially excited when this machine's time ticks.
    /// </summary>
    internal void RegisterPotentialExcitation(Neuron neuron);
    /// <summary>
    /// Sets the specified axon to emit charge at the specified time.
    /// </summary>
    /// <param name="timeOfDelivery">The time the emit is to be delivered at the end of its axon. </param>
    internal void AddEmitAction(int timeOfDelivery, Axon axon);
}
