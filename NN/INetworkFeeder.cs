using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

/// <summary>
/// Determines when the input axons fire, including before the network is run.
/// </summary>
public interface INetworkFeeder
{
    internal const int INITIALIZATION_TIME = 0;
    public static INetworkFeeder CreateRandom(int seed)
    {
        return CreateRandom(new Random(seed));
    }
    public static INetworkFeeder CreateRandom(Random random)
    {
        return new RandomNetworkPrimer(random);
    }
    public static INetworkFeeder CreateUniformActivator()
    {
        return new UniformNetworkPrimer();
    }

    /// <summary>
    /// Gets the input axons to activate when on start.
    /// </summary>
    internal void Activate(IReadOnlyList<Axon> axons, IMachine machine);

    // TODO:
    // support accepting a large immutable list of axon firings
    // support a pattern that allows for reacting to outputs
}
