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
    /// <inheritdoc cref="DeterministicFeeder.DeterministicFeeder(IEnumerable{IReadOnlyList{bool}})"/>
    public static INetworkFeeder CreateDeterministicFeeder(IEnumerable<IReadOnlyList<bool>> inputs)
    {
        return new PredeterminedFeeder(inputs);
    }

    /// <summary>
    /// Feeds the input axons. Gets called every time step.
    /// </summary>
    public void Feed(IMachine machine, EventArgs e);
}
