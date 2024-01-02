using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface INetworkFactory
{
    /// <summary>
    /// Creates a <see cref="INetwork"/> and <see cref="IMachine"/> representing a neural network and its
    /// </summary>
    /// <param name="maxTime"></param>
    /// <returns></returns>
    public (IMachine, INetwork) Create(int? maxTime = null)
    {
        var clock = IClock.Create(maxTime);
        var network = INetwork.Create(this.NeuronTypes, this.InputCount, this.OutputCount, this.GetAxonConnection, clock);
        var machine = IMachine.Create(network);
        this.InputPrimer.Activate(network.Inputs, machine);
        return (machine, network);
    }

    /// <summary>
    /// The number of input axons.
    /// </summary>
    public int InputCount { get; }
    /// <summary>
    /// The number of output neurons.
    /// </summary>
    public int OutputCount { get; }
    /// <summary>
    /// The neuron types of the network.
    /// </summary>
    /// <remarks>Must have at least <see cref="OutputCount"/> elements.</remarks>
    public IReadOnlyList<INeuronType> NeuronTypes { get; }
    /// <summary>
    /// Gets the initialization data per axon.
    /// </summary>
    /// <param name="neuronFromIndex">The index in <see cref="NeuronTypes"/> of the axon's start neuron. -1 if it's an input axon.</param>
    /// <param name="neuronToIndex">The index in <see cref="NeuronTypes"/> of the axon's end neuron.</param>
    /// <returns><see langword="null"/> if there's no connection between the two specified neurons; otherwise initial data length and weights for the requested axon.</returns>
    public IAxonInitialization? GetAxonConnection(int neuronFromIndex, int neuronToIndex);
    /// <summary>
    /// Gets an object representing the data to be fed to the network.
    /// </summary>
    public INetworkFeeder InputPrimer { get; }
}
