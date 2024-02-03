using JBSnorro.NN;
using JBSnorro.NN.Internals;
using Assert = Xunit.Assert;
using VariableNeuronType = JBSnorro.NN.Internals.VariableNeuronType;

namespace Tests.JBSnorro.NN;

public class NetworkTests
{
    [Fact]
    public void CreateNetwork()
    {
        INetwork.Create(NeuronTypes.OnlyOne,
                        outputCount: 1,
                        (i, j) => i == -1 ? InputAxonInitialization.Input : null,
                        IClock.Create(maxTime: null));
    }
    [Fact]
    public void CreateNetworkViaFactory()
    {
        INetworkFactory factory = new MockNetworkFactory(new INeuronType[0], 0, 0, (i, j) => null, INetworkFeeder.CreateUniformActivator());
        factory.Create();
    }
    record MockNetworkFactory(
        IReadOnlyList<INeuronType> NeuronTypes,
        int NeuronCount,
        int OutputCount,
        GetAxonConnectionDelegate getConnections,
        INetworkFeeder InputFeeder
    ) : INetworkFactory
    {
        IAxonInitialization? INetworkFactory.GetAxonConnection(int neuronFromIndex, int neuronToIndex) => getConnections(neuronFromIndex, neuronToIndex);
    }


    [Fact]
    public void RunActivatedNetworkOfOne()
    {
        var connections = new IAxonInitialization?[1, 1] { { null } };
        var network = INetwork.Create(NeuronTypes.OnlyOne,
                                      outputCount: 1,
                                      (i, j) => i == -1 ? InputAxonInitialization.Input : null,
                                      IClock.Create(maxTime: null));

        var machine = IMachine.Create(network, (_, _) => null);
        INetworkFeeder.CreateUniformActivator().Activate(((Network)network).Inputs, machine);

        var output = machine.Run(1);

        Assert.Equal(output, new float[] { 1 });
    }


    [Fact]
    public void TestNeuronDeactivatesAfterActivation()
    {
        var connections = new IAxonInitialization?[1, 1] { { null } };
        var network = INetwork.Create(NeuronTypes.OnlyOne,
                                      outputCount: 1,
                                      (i, j) => i == -1 ? InputAxonInitialization.Input : null,
                                      IClock.Create(maxTime: null));

        var machine = IMachine.Create(network, (_, _) => null);
        INetworkFeeder.CreateUniformActivator().Activate(((Network)network).Inputs, machine);

        var output = machine.RunCollect(2);

        Assert.Equal(output, new float[][] { new float[] { 1 }, new float[] { 0 } });
    }
    [Fact]
    public void TestNeuronCanActivateSelf()
    {
        var network = INetwork.Create(NeuronTypes.OnlyOne,
                                      outputCount: 1,
                                      (i, j) => i == -1 ? InputAxonInitialization.Input : MockAxonType.LengthTwo,
                                      IClock.Create(maxTime: null));

        var machine = IMachine.Create(network, (_, _) => null);
        INetworkFeeder.CreateUniformActivator().Activate(((Network)network).Inputs, machine);

        var output = machine.RunCollect(3);
        Assert.Equal(actual: output, expected: new float[][] { new float[] { 1 }, new float[] { 0 }, new float[] { 1 } });
    }
    [Fact]
    public void StressTest()
    {
        var random = new Random(Seed: 0);
        const int neuronCount = 100;
        const int inputCount = 5;
        const int outputCount = 5;
        const int maxTime = 15;
        const float initializationChance = 1f;
        const float connectionChance = 0.5f;

        var connections = MockAxonType.CreateRandom(neuronCount, connectionChance, random);

        var neuronTypes = new INeuronType[neuronCount];
        for (int i = 0; i < neuronCount; i++)
        {
            neuronTypes[i] = random.Next(3) switch
            {
                0 => NeuronTypes.A,
                1 => NeuronTypes.B,
                2 => NeuronTypes.C,
                _ => throw new Exception()
            };
        }
        var randomInitialization = new IAxonInitialization[inputCount];
        for (int i = 0; i < inputCount; i++)
        {
            randomInitialization[i] = InputAxonInitialization.Create(new float[] { random.NextSingle() < initializationChance ? 1 : 0 });
        }

        var network = INetwork.Create(neuronTypes,
                                      outputCount,
                                      (i, j) => i == -1 ? (j < randomInitialization.Length ? randomInitialization[j] : null) : connections[i, j],
                                      IClock.Create(maxTime: null));

        var machine = IMachine.Create(network, (_, _) => null);
        INetworkFeeder.CreateRandom(random).Activate(((Network)network).Inputs, machine);

        var output = machine.RunCollect(maxTime);
        for (int t = 0; t < maxTime; t++)
        {
            Console.Write("[");
            for (int i = 0; i < outputCount; i++)
            {
                Console.Write(output[t][i].ToString("n2"));
                if (i != outputCount - 1)
                    Console.Write(", ");

            }
            Console.WriteLine("]");
        }
    }

}
public class NeuronTypeTests
{
    [Fact]
    public void TestDecaySequence()
    {
        var n = (VariableNeuronType)INeuronType.CreateVariable(
            new[] { (2, 0.5f) },
            new (int, float)[0]
        );

        var sequence = n.GetNoActivationDecaySequence().Take(4).Select(f => f.ToString("n2")).ToList();
        Assert.Equal(sequence, new[] { "0.50", "0.50", "0.00", "0.00" });
    }

    [Fact]
    public void TestCumulativeDecaySequence()
    {
        var neuron = (VariableNeuronType)INeuronType.CreateVariable(
            new[] { (3, 0.5f) },
            new (int, float)[0]
        );

        var sequence = neuron.GetNoActivationCumulativeDecaySequence().Take(4).Select(f => f.ToString("n2")).ToList();
        Assert.Equal(sequence, new[] { "0.50", "0.25", "0.12", "0.00" });
    }


    [Fact]
    public void TestNeuronInitialChargeDecay()
    {
        var type = INeuronType.CreateVariable(
            new[] { (2, 0.5f) },
            new (int, float)[0]
        );

        var neuron = new Neuron(type, initialCharge: 1);
        var charges = new float[4];
        for (int t = 0; t < charges.Length; t++)
        {
            neuron.Decay(t);
            charges[t] = neuron.Charge;
        }
        // these are the charges at the ends of timesteps just before clearance
        // given that the initial axon fires at time 0 and reaches the neuron between time 0 and 1,
        // the charge at the end of time 0 is 1f
        Assert.Equal(charges, new[] { 1f, 0.50f, 0.25f, 0 });
    }

    [Fact]
    public void TestNeuronInputChargeDecay()
    {
        var type = new VariableNeuronType(
            new (int, float)[0],
            new[] { (2, 0.5f) }  // in contrast to the test above, activation is triggered in this one, hence we use that list
        );

        var neuron = new Neuron(type, 0);
        // simulate without triggering Decay
        neuron.Receive(1, Machines.AtTime0);
        neuron.Excite(Machines.AtTime0);

        var charges = new float[4];
        for (int t = 0; t < charges.Length; t++)
        {
            neuron.Decay(t);
            charges[t] = neuron.Charge;
        }
        // these are the charges at the ends of timesteps just before clearance
        // given that the initial axon fires at time 0 and reaches the neuron between time 0 and 1,
        // the charge at the end of time 0 is 1f
        Assert.Equal(charges, new[] { 1f, 0.50f, 0.25f, 0.25f });
    }

    [Fact]
    public void TestClockPassedToFeedbackIsAtEndOfTimeStep()
    {
        var network = INetwork.Create(new[] { INeuronType.NoRetentionNeuronType }, 1, (_, _) => null, IClock.Create(maxTime: 3));
        var feedbackTimes = new List<int>();
        Feedback? GetFeedback(ReadOnlySpan<float> latestOutput, IReadOnlyClock clock)
        {
            feedbackTimes.Add(clock.Time);
            return null;
        }

        // Act
        IMachine.Create(network, GetFeedback).Run();

        Assert.Equal(feedbackTimes, new[] { 0, 1, 2 });
    }
}
