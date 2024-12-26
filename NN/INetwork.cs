using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

/// <summary>
/// Represents a graph of nodes and edges (or neurons and axons).
/// There is a special set of input axons that are controlled from outside and connect to neurons.
/// There is also a special set of output neurons which are considerd to be the output of the network.
/// </summary>
public interface INetwork
{
    /// <remarks>If you use this method for creating a Network you need to initialize the input axons yourself.</remarks>
    internal static INetwork Create(IReadOnlyList<INeuronType> neuronTypes,
                                    int outputCount,
                                    GetAxonConnectionDelegate getConnection,
                                    IReadOnlyClock clock)
    {
        return new Network(neuronTypes, outputCount, getConnection, clock);
    }

    public IReadOnlyClock Clock { get; }
    /// <summary>
    /// Gets the output of this machine.
    /// </summary>
    public float[] Output { get; }

    internal IReadOnlyList<Axon> Inputs { get; }
    internal IReadOnlyList<Axon> Axons { get; }
    internal IClock MutableClock => (IClock)Clock;
    internal void Process(IFeedback feedback);
    internal void Decay();
    internal IReadOnlyList<Neuron> Neurons { get; }
}

