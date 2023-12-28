using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface INetwork
{
    public static INetwork Create(INeuronType[] nodeTypes,
                                  int inputCount,
                                  int outputCount,
                                  AxonType?[,] connections,
                                  INetworkInitializer initializer,
                                  int? maxTime)
    {
        return new Network(nodeTypes, inputCount, outputCount, connections, initializer, maxTime);
    }

    /// <summary>
    /// Gets the output of this machine.
    /// </summary>
    public float[] Output { get; }
    
    internal void Initialize(IMachine machine);
    internal void Process(Feedback feedback, int time);
    internal void Decay(int time);
}

public interface INetworkInitializer
{
    internal const int INITIALIZATION_TIME = 0;
    public static INetworkInitializer CreateRandom(int seed)
    {
        return CreateRandom(new Random(seed));
    }
    public static INetworkInitializer CreateRandom(Random random)
    {
        return new RandomNetworkInitializer(random);
    }
    public static INetworkInitializer CreateUniformActivator()
    {
        return new UniformNetworkInitializer();
    }

    /// <summary>
    /// Gets the input axons to activate when on start.
    /// </summary>
    internal void Activate(IReadOnlyList<Axon> axons, IMachine machine);
}
sealed class RandomNetworkInitializer : INetworkInitializer
{
    public Random Random { get; }

    public RandomNetworkInitializer(Random random)
    {
        this.Random = random;
    }

    public void Activate(IReadOnlyList<Axon> axons, IMachine machine)
    {
        foreach (var axon in axons)
        {
            bool activate = Random.Next(2) == 0;
            if (activate)
            {
                machine.AddEmitAction(INetworkInitializer.INITIALIZATION_TIME, axon);
            }
        }
    }
}

/// <summary>
/// Activates all input axons.
/// </summary>
sealed class UniformNetworkInitializer : INetworkInitializer
{
    public void Activate(IReadOnlyList<Axon> axons, IMachine machine)
    {
        foreach (var axon in axons)
        {
            machine.AddEmitAction(INetworkInitializer.INITIALIZATION_TIME, axon);
        }
    }
}