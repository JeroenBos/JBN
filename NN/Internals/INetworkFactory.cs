using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface INetworkFactory
{
    public (IMachine, INetwork) Create(int? maxTime = null)
    {
        var clock = IClock.Create(maxTime);
        var network = INetwork.Create(this.NeuronTypes, this.InputCount, this.OutputCount, this.GetAxonConnection, clock);
        var machine = IMachine.Create(network);
        this.InputPrimer.Activate(network.Inputs, machine);
        return (machine, network);
    }

    public int InputCount { get; }
    public int OutputCount { get; }
    public IReadOnlyList<INeuronType> NeuronTypes { get; }
    public IAxonInitialization? GetAxonConnection(int neuronFromIndex, int neuronToIndex);
    public INetworkFeeder InputPrimer { get; }
}
