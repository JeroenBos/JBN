namespace JBSnorro.NN.Internals;

// nomenclature: a network primer is a network feeder that only feeds before the network has run

/// <summary>
/// Activates the input axons randomly at the start.
/// </summary>
internal sealed class RandomNetworkPrimer : INetworkFeeder
{
    public Random Random { get; }

    public RandomNetworkPrimer(Random random)
    {
        this.Random = random;
    }

    public void OnFeed(IMachine machine, OnFeedEventArgs e)
    {
        if (e.Time != IReadOnlyClock.UNSTARTED)
            return;

        foreach (var axon in e.inputAxons)
        {
            bool activate = Random.Next(2) == 0;
            if (activate)
            {
                machine.AddEmitAction(INetworkFeeder.INITIALIZATION_TIME, axon);
            }
        }
    }
}

/// <summary>
/// Activates all input axons at the start.
/// </summary>
internal sealed class UniformNetworkPrimer : INetworkFeeder
{
    public void OnFeed(IMachine machine, OnFeedEventArgs e)
    {
        if (e.Time != IReadOnlyClock.UNSTARTED)
            return;

        foreach (var axon in e.inputAxons)
        {
            machine.AddEmitAction(INetworkFeeder.INITIALIZATION_TIME, axon);
        }
    }
}

internal sealed class PredeterminedFeeder : INetworkFeeder
{
    // null means finished
    private IEnumerator<IReadOnlyList<bool>>? inputs;
    private int lastTime = IReadOnlyClock.UNSTARTED - 1;

    /// <summary>
    /// Creates a deterministic sequence of input neuron excitations.
    /// </summary>
    /// <param name="inputs">A sequence of indications whether input neurons fire. The first yielded list represents the time -1, etc.</param>
    public PredeterminedFeeder(IEnumerable<IReadOnlyList<bool>> inputs)
    {
        this.inputs = inputs.GetEnumerator();
    }


    void INetworkFeeder.OnFeed(IMachine machine, OnFeedEventArgs e)
    {
        if (inputs is null || !inputs.MoveNext())
        {
            return;
        }
        if (inputs.Current.Count != e.inputAxons.Count)
        {
            throw new InvalidOperationException($"Each yielded set must have the exact same number of element as there are input neurons (={e.inputAxons.Count}); got {inputs.Current.Count}");
        }
        var currentTime = machine.Clock.Time;
        if (lastTime + 1 != currentTime)
        {
            throw new InvalidOperationException($"{nameof(INetworkFeeder)}.{nameof(INetworkFeeder.OnFeed)} must be called every timestep");
        }
        this.lastTime = currentTime;


        foreach (var (axon, input) in e.inputAxons.Zip(inputs.Current))
        {
            if (input)
            {
                machine.AddEmitAction(currentTime + 1, axon);
            }
        }
    }
}
