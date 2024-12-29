using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

/// <summary>
/// Represents a graph of nodes and edges (or neurons and axons).
/// There is a special set of input axons that are controlled from outside and connect to neurons.
/// There is also a special set of output neurons which are considerd to be the output of the network.
/// </summary>
public interface INetwork
{
    /// <summary>
    /// Creates a network and machine to operate it.
    /// </summary>
    /// <param name="neuronTypes">The types of the neurons to create in the network.</param>
    /// <param name="outputCount">The number of neurons at the end of <paramref name="neuronTypes"/> that are output neurons (i.e. whose charge will be reported). </param>
    /// <param name="getAxon">A function specifying connections between the neurons. The arguments are up to <paramref name="neuronTypes"/>.Count), the first parameter accepts -1 indicating input axon. </param>
    /// <param name="feeder">A function specifing when input axons are triggered. </param>
    /// <param name="maxTime">The maximum time the machine is allowed to run. </param>
    public static (IMachine Machine, INetwork Network) Create(
        IReadOnlyList<INeuronType> neuronTypes,
        int outputCount,
        GetAxonConnectionDelegate getAxon,
        INetworkFeeder feeder,
        int? maxTime = null)
    {
        var clock = IClock.Create(maxTime);
        var network = Create(neuronTypes, outputCount, getAxon, clock);
        var machine = IMachine.Create(network, feeder);
        return (machine, network);
    }

    /// <remarks>If you use this method for creating a Network you need to initialize the input axons yourself.</remarks>
    internal static INetwork Create(IReadOnlyList<INeuronType> neuronTypes,
                                    int outputCount,
                                    GetAxonConnectionDelegate getConnection,
                                    IReadOnlyClock? clock = null)
    {
        return new Network(neuronTypes, outputCount, getConnection, clock ?? IClock.Create(maxTime: null));
    }

    public IReadOnlyClock Clock { get; }
    /// <summary>
    /// Gets the output of this machine.
    /// </summary>
    public float[] Output { get; }

    public int InputAxonCount => Inputs.Count;
    internal IReadOnlyList<Axon> Inputs { get; }
    internal IReadOnlyList<Axon> Axons { get; }
    internal IClock MutableClock => (IClock)Clock;
    internal void Process(IFeedback feedback);
    internal void Decay(IMachine machine);
    internal IReadOnlyList<Neuron> Neurons { get; }
}

