namespace JBSnorro.NN.Internals;

internal sealed class RandomNetworkInitializer : INetworkInitializer
{
    public Random Random { get; }

    public RandomNetworkInitializer(Random random)
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
                machine.AddEmitAction(INetworkInitializer.INITIALIZATION_TIME, axon);
            }
        }
    }
}

/// <summary>
/// Activates all input axons.
/// </summary>
internal sealed class UniformNetworkInitializer : INetworkInitializer
{
    public void Activate(IReadOnlyList<Axon> axons, IMachine machine)
    {
        foreach (var axon in axons)
        {
            machine.AddEmitAction(INetworkInitializer.INITIALIZATION_TIME, axon);
        }
    }
}
