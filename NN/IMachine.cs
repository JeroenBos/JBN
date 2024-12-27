using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface IMachine
{
    public static IMachine Create(INetwork network, INetworkFeeder? feed = null)
    {
        var machine = new Machine(network);
        if (feed is not null)
        {
            machine.OnTicked += feed.Feed;
            feed.Feed(machine, new OnTickEventArgs(IReadOnlyClock.UNSTARTED, network.Inputs.Count));
        }
        return machine;
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
            result.Add([.. e.Output]);
        }
    }

    /// <summary>
    /// Indicates the current machine's time.
    /// </summary>
    public IReadOnlyClock Clock { get; }
    /// <summary>
    /// Gets the network this machine is associated with.
    /// </summary>
    public INetwork Network { get; }
    /// <summary>
    /// Excites the specified input axon.
    /// </summary>
    public void Excite(int inputAxonIndex);

    /// <summary>
    /// Gets the current charges of the output neurons of the network.
    /// </summary>
    internal float[] Output { get; }
    /// <summary>
    /// Registers a <see cref="Neuron"/> that is potentially excited when this machine's time ticks.
    /// </summary>
    internal void RegisterPotentialExcitation(Neuron neuron);
}
