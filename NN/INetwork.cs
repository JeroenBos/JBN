using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface INetwork
{
    public static INetwork Create(INeuronType[] nodeTypes,
                                  int inputCount,
                                  int outputCount,
                                  IAxonType?[,] connections,
                                  INetworkInitializer initializer,
                                  int? maxTime)
    {
        return new Network(nodeTypes, inputCount, outputCount, connections, initializer, maxTime);
    }

    public IReadOnlyClock Clock { get; }
    /// <summary>
    /// Gets the output of this machine.
    /// </summary>
    public float[] Output { get; }
    
    internal IReadOnlyList<Axon> Axons { get; }
    internal void Initialize(IMachine machine);
    internal IClock MutableClock => (IClock)Clock;
    internal void Process(Feedback feedback, int time);
    internal void Decay(int time);
}
