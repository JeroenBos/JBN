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

    public void Activate(IReadOnlyList<Axon> axons, IMachine machine)
    {
        foreach (var axon in axons)
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
    public void Activate(IReadOnlyList<Axon> axons, IMachine machine)
    {
        foreach (var axon in axons)
        {
            machine.AddEmitAction(INetworkFeeder.INITIALIZATION_TIME, axon);
        }
    }
}
