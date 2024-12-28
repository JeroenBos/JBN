using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

/// <summary>
/// Determines when the input axons fire, including before the network is run.
/// </summary>
public interface INetworkFeeder
{
    internal const int INITIALIZATION_TIME = 0;
    /// <summary>
    /// Creates a <see cref="INetworkFeeder"/> that that primes the network randomly.
    /// </summary>
    public static INetworkFeeder CreateRandomPrimer(int seed)
    {
        return CreateRandomPrimer(new Random(seed));
    }
    /// <summary>
    /// Creates a <see cref="INetworkFeeder"/> that that primes the network randomly.
    /// </summary>
    public static INetworkFeeder CreateRandomPrimer(Random random)
    {
        return new RandomNetworkPrimer(random);
    }
    /// <summary>
    /// Creates a <see cref="INetworkFeeder"/> that that primes the network uniformly.
    /// </summary>
    public static INetworkFeeder CreateUniformPrimer()
    {
        return new UniformNetworkPrimer();
    }
    /// <inheritdoc cref="PredeterminedFeeder(IEnumerable{IReadOnlyList{bool}})"/>
    public static INetworkFeeder CreateDeterministicFeeder(IEnumerable<IReadOnlyList<bool>> inputs)
    {
        return new PredeterminedFeeder(inputs);
    }

    /// <summary>
    /// Feeds the input axons. Gets called every time step.
    /// </summary>
    public void Feed(IMachine machine, EventArgs e);
}
