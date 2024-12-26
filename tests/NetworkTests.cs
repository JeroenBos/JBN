using JBSnorro.NN;
using JBSnorro.NN.Internals;
using Assert = Xunit.Assert;
using VariableNeuronType = JBSnorro.NN.Internals.VariableNeuronType;

namespace Tests.JBSnorro.NN;

public class NetworkTests
{
    GetFeedbackDelegate noFeedback = (_, _) => null;
    [Fact]
    public void CreateNetwork()
    {
        INetwork.Create(NeuronTypes.OnlyOne,
                        outputCount: 1,
                        (i, j) => i == -1 ? InputAxonType.Instance : null,
                        IClock.Create(maxTime: null));
    }
    [Fact]
    public void CreateNetworkViaFactory()
    {
        INetworkFactory factory = new MockNetworkFactory([], 0, 0, (i, j) => null, INetworkFeeder.CreateUniformActivator());
        factory.Create();
    }
    record MockNetworkFactory(
        IReadOnlyList<INeuronType> NeuronTypes,
        int NeuronCount,
        int OutputCount,
        GetAxonConnectionDelegate getConnection,
        INetworkFeeder InputFeeder
    ) : INetworkFactory
    {
        IAxonType? INetworkFactory.GetAxonConnection(int neuronFromIndex, int neuronToIndex) => getConnection(neuronFromIndex, neuronToIndex);
    }


    [Fact]
    public void RunActivatedNetworkOfOne()
    {
        var connections = new IAxonType?[1, 1] { { null } };
        var network = INetwork.Create(NeuronTypes.OnlyOne,
                                      outputCount: 1,
                                      (i, j) => i == -1 ? InputAxonType.Instance : null,
                                      IClock.Create(maxTime: null));

        var machine = IMachine.Create(network, noFeedback);
        INetworkFeeder.CreateUniformActivator().Activate(network.Inputs, machine);

        var output = machine.Run(maxTime: 1);

        Assert.Equal(output, [1f]);
    }


    [Fact]
    public void TestNeuronDeactivatesAfterExcitation()
    {
        var connections = new IAxonType?[1, 1] { { null } };
        var network = INetwork.Create(NeuronTypes.OnlyOne,
                                      outputCount: 1,
                                      (i, j) => i == -1 ? InputAxonType.Instance : null,
                                      IClock.Create(maxTime: null));

        var machine = IMachine.Create(network, noFeedback);
        INetworkFeeder.CreateUniformActivator().Activate(network.Inputs, machine);

        var output = machine.RunCollect(2);

        Assert.Equal(actual: output, expected: [[1f], [0f]]);
    }
    [Fact]
    public void TestNeuronCanActivateSelf()
    {
        var network = INetwork.Create(NeuronTypes.OnlyOne,
                                      outputCount: 1,
                                      (i, j) => i == -1 ? InputAxonType.Instance : MockAxonType.LengthTwo,
                                      IClock.Create(maxTime: null));

        var machine = IMachine.Create(network, noFeedback);
        INetworkFeeder.CreateUniformActivator().Activate(network.Inputs, machine);

        var output = machine.RunCollect(3);
        Assert.Equal(actual: output, expected: [[1f], [0f], [1f]]);
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
        var randomInitialization = new IAxonType[inputCount];
        for (int i = 0; i < inputCount; i++)
        {
            randomInitialization[i] = InputAxonType.Create(new float[] { random.NextSingle() < initializationChance ? 1 : 0 });
        }

        var network = INetwork.Create(neuronTypes,
                                      outputCount,
                                      (i, j) => i == -1 ? (j < randomInitialization.Length ? randomInitialization[j] : null) : connections[i, j],
                                      IClock.Create(maxTime: null));

        var machine = IMachine.Create(network, noFeedback);
        INetworkFeeder.CreateRandom(random).Activate(network.Inputs, machine);

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
            [(2, 0.5f)],
            []
        );

        var sequence = n.GetNoExcitationDecaySequence().Take(4).Select(f => f.ToString("n2").Replace(',', '.')).ToList();
        Assert.Equal(sequence, new[] { "0.50", "0.50", "0.00", "0.00" });
    }

    [Fact]
    public void TestCumulativeDecaySequence()
    {
        var neuron = (VariableNeuronType)INeuronType.CreateVariable(
            [(3, 0.5f)],
            []
        );

        var sequence = neuron.GetNoExcitationCumulativeDecaySequence().Take(4).Select(f => f.ToString("n2").Replace(',', '.')).ToList();
        Assert.Equal(sequence, new[] { "0.50", "0.25", "0.12", "0.00" });
    }


    [Fact]
    public void TestNeuronInitialChargeDecay()
    {
        var type = INeuronType.CreateVariable(
            [(maxDt: 2, decay: 0.5f)],
            []
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
        Assert.Equal(charges, [1f, 0.50f, 0.25f, 0]);
    }

    [Fact]
    public void TestNeuronInputChargeDecay()
    {
        var type = new VariableNeuronType(
            [],
            [(maxDt: 2, decay: 0.5f)] // in contrast to the test above, activation is triggered in this one, hence we use that list
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
        Assert.Equal(charges, [1f, 0.50f, 0.25f, 0.25f]);
    }

    [Fact]
    public void Clock_passed_to_Feedback_should_be_at_end_of_timestep()
    {
        const int maxTime = 3;
        var network = INetwork.Create([INeuronType.NoRetentionNeuronType], 1, (_, _) => null, IClock.Create(maxTime));
        List<int> feedbackTimes = [];
        IFeedback? GetFeedback(ReadOnlySpan<float> latestOutput, IReadOnlyClock clock)
        {
            feedbackTimes.Add(clock.Time);
            return null;
        }

        // Act
        IMachine.Create(network, GetFeedback).Run();

        Assert.Equal(feedbackTimes, [0, 1, 2]);
    }

    [Fact]
    public void TestThatInputWithSingleWeightIsAcceptedWhenTheNetworkUsesMultipleWeights()
    {
        // the network is one input axon, one hidden neuron and one output neuron, receiving linea recta from the hidden neuron.
        const int INPUT_NEURON = IAxonType.FROM_INPUT;
        const int HIDDEN_NEURON = 0;
        const int OUTPUT_NEURON = 1;
        float[] actual = Array.Empty<float>();
        var intermediateAxon = new AxonTypeThatUsesTwoWeights([1f, (float)Math.PI] /*explicitly has two elements*/, currentWeightsCallback);
        IAxonType? getConnections(int from, int to) => (from, to) switch
        {
            (INPUT_NEURON, HIDDEN_NEURON) => InputAxonType.Create([1f] /*explicitly has one element*/),
            (HIDDEN_NEURON, OUTPUT_NEURON) => intermediateAxon,
            _ => null
        };
        var network = INetwork.Create(Enumerable.Repeat(INeuronType.NoRetentionNeuronType, 2).ToArray(), outputCount: 1, getConnections, IClock.Create(maxTime: 2));
        var machine = IMachine.Create(network, (_, _) => new MockFeedback());
        void currentWeightsCallback(float[] currentWeights)
        {
            actual = currentWeights.ToArray();
        }

        // Act
        machine.Run();

        Assert.Equal(2, actual.Length);
    }

    class AxonTypeThatUsesTwoWeights(IReadOnlyList<float> initialWeights, Action<float[]> currentWeightsCallback) : IAxonType
    {
        public int Length => 1;
        public IReadOnlyList<float> InitialWeights => initialWeights;


        public void UpdateWeights(float[] currentWeights, int timeSinceLastExcitation, float averageTimeBetweenExcitations, int excitationCount, IFeedback feedback)
        {
            currentWeightsCallback(currentWeights);
        }
    }
    class MockFeedback : IFeedback
    {
        public bool Stop => false;
    }
}
