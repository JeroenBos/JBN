using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface INetworkInitializer
{
    internal const int INITIALIZATION_TIME = 0;
    public static INetworkInitializer CreateRandom(int seed)
    {
        return CreateRandom(new Random(seed));
    }
    public static INetworkInitializer CreateRandom(Random random)
    {
        return new RandomNetworkInitializer(random);
    }
    public static INetworkInitializer CreateUniformActivator()
    {
        return new UniformNetworkInitializer();
    }

    /// <summary>
    /// Gets the input axons to activate when on start.
    /// </summary>
    internal void Activate(IReadOnlyList<Axon> axons, IMachine machine);
}
