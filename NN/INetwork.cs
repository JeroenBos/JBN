using System.Runtime.CompilerServices;
using JBSnorro.NN.Internals;
using Either = JBSnorro.Either<JBSnorro.NN.INeuron, JBSnorro.NN.IAxonBuilder>;

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
    /// <param name="outputCount">The number of neurons at the end of <paramref name="seeder"/> that are output neurons (i.e. whose charge will be reported). </param>
    /// <param name="feeder">A function specifing when input axons are triggered. </param>
    /// <param name="maxTime">The maximum time the machine is allowed to run. </param>
    /// <param name="update"></param>
    public static (IMachine Machine, INetwork Network) Create(
        IEnumerable<Either> seeder,
        int outputCount,
        INetworkFeeder feeder,
        UpdateWeightsDelegate update,
        int? maxTime = null)
    {
        var network = Create(seeder, outputCount, clock: IClock.Create(maxTime));
        var machine = IMachine.Create(network, feeder);
        return (machine, network);
    }

    /// <summary>
    /// For testing only! (uses deprecated <see cref="GetAxonConnectionDelegate"/>)
    /// Creates a network and machine to operate it.
    /// </summary>
    /// <param name="neurons">The types of the neurons to create in the network.</param>
    /// <param name="outputCount">The number of neurons at the end of <paramref name="neurons"/> that are output neurons (i.e. whose charge will be reported). </param>
    /// <param name="getAxon">A function specifying connections between the neurons. The arguments are up to <paramref name="neurons"/>.Count), the first parameter accepts -1 indicating input axon. </param>
    /// <param name="feeder">A function specifing when input axons are triggered. </param>
    /// <param name="maxTime">The maximum time the machine is allowed to run. </param>
    internal static INetwork Create(
        IReadOnlyList<INeuron> neurons,
        int outputCount,
        GetAxonConnectionDelegate getAxon,
        IReadOnlyClock? clock = null)
    {
        var axons = Enumerable2D.Range(IAxonBuilder.FROM_INPUT, neurons.Count - 1, 0, neurons.Count - 1, (i, j) => getAxon(i, j)).Where(x => x is not null);
        var seeder = Enumerable.Concat(neurons.Select(neuronType => new Either(neuronType)), axons.Select(axonBuilder => new Either(axonBuilder!)));
        return Create(seeder, outputCount, clock);
    }

    /// <remarks>If you use this method for creating a Network you need to initialize the input axons yourself.</remarks>
    internal static INetwork Create(IEnumerable<Either> seeder,
                                    int outputCount,
                                    IReadOnlyClock? clock = null)
    {
        return new Network(seeder, outputCount, clock ?? IClock.Create(maxTime: null));
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
