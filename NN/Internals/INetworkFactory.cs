using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

internal interface INetworkFactory : INetworkFactory<IFeedback> { }
public interface INetworkFactory<TFeedback> where TFeedback : class, IFeedback
{
    /// <summary>
    /// Creates a <see cref="INetwork"/> and <see cref="IMachine"/> representing a neural network and its
    /// </summary>
    /// <param name="maxTime"></param>
    /// <returns></returns>
    public (IMachine Machine, INetwork Network) Create(int? maxTime = null)
    {
        var clock = IClock.Create(maxTime);
        var network = INetwork.Create(this.NeuronTypes, this.OutputCount, this.GetAxonConnection, clock);
        var machine = IMachine.Create(network, this.GetFeedback);
        this.InputFeeder.Activate(network.Inputs, machine);
        return (machine, network);
    }

    /// <summary>
    /// A type of neuron for each neuron in the network.
    /// </summary>
    /// <remarks>Must have at least <see cref="OutputCount"/> elements.</remarks>
    public IReadOnlyList<INeuronType> NeuronTypes { get; }
    /// <summary>
    /// The number of output neurons.
    /// </summary>
    public int OutputCount { get; }
    /// <summary>
    /// Gets the initialization data per axon.
    /// </summary>
    /// <param name="neuronFromIndex">The index in <see cref="NeuronTypes"/> of the axon's start neuron. <see cref="IAxonType.FROM_INPUT"/> if it's an input axon.</param>
    /// <param name="neuronToIndex">The index in <see cref="NeuronTypes"/> of the axon's end neuron.</param>
    /// <returns><see langword="null"/> if there's no connection between the two specified neurons; otherwise initial data length and weights for the requested axon.</returns>
    public IAxonInitialization? GetAxonConnection(int neuronFromIndex, int neuronToIndex);
    /// <summary>
    /// Gets an object representing the data to be fed to the network.
    /// </summary>
    public INetworkFeeder InputFeeder { get; }
    /// <summary>
    /// Gets (external) feedback for the given output.
    /// </summary>
    /// <param name="latestOutput">The output of the current round.</param>
    /// <param name="clock">The current clock, at the end of the current time step.</param>
    /// <returns>Feedback to be incorporated by the network; <see langword="null"/> indicates there is no feedback and the network should continue operating as is.</returns>
    public TFeedback? GetFeedback(ReadOnlySpan<float> latestOutput, IReadOnlyClock clock) => null;
}
