﻿using JBSnorro.NN.Internals;

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
    /// Activates the axons (depends on the machine's clock time).
    /// </summary>
    internal void Activate(IReadOnlyList<Axon> inputAxons, IMachine machine);

    // TODO:
    // support a pattern that allows for reacting to outputs
}
