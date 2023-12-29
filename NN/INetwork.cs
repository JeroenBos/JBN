using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface INetwork
{
    public static INetwork Create(INeuronType[] nodeTypes,
                                  int inputCount,
                                  int outputCount,
                                  IAxonInitialization?[,] connections,
                                  IReadOnlyClock clock)
    {
        // if you use this method for creating a Network you need to initialize the input axons yourself
        return new Network(nodeTypes, inputCount, outputCount, connections, clock);
    }

    public IReadOnlyClock Clock { get; }
    /// <summary>
    /// Gets the output of this machine.
    /// </summary>
    public float[] Output { get; }
    
    internal IReadOnlyList<Axon> Axons { get; }
    internal IClock MutableClock => (IClock)Clock;
    internal void Process(Feedback feedback, int time);
    internal void Decay(int time);
}
