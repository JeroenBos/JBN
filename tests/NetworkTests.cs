using JBSnorro;
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
        INetwork.Create(NeuronTypes.OnlyOne.Select(INeuron.Create).ToList(),
                        outputCount: 1,
                        UpdateWeightsDelegateExtensions.Empty,
                        (i, j) => i == -1 ? InputAxonType.Create(j) : null);
    }


    [Fact]
    public void RunActivatedNetworkOfOne()
    {
        var connections = new IAxonBuilder?[1, 1] { { null } };
        var network = INetwork.Create(NeuronTypes.OnlyOne.Select(INeuron.Create).ToList(),
                                      outputCount: 1,
                                      UpdateWeightsDelegateExtensions.Empty,
                                      (i, j) => i == -1 ? InputAxonType.Create(j) : null);

        var machine = IMachine.Create(network, INetworkFeeder.CreateUniformPrimer());

        var output = machine.Run(maxTime: 1);

        Assert.Equal(output, [1f]);
    }


    [Fact]
    public void TestNeuronDeactivatesAfterExcitation()
    {
        var connections = new IAxonBuilder?[1, 1] { { null } };
        var network = INetwork.Create(NeuronTypes.OnlyOne.Select(INeuron.Create).ToList(),
                                      outputCount: 1,
                                      UpdateWeightsDelegateExtensions.Empty,
                                      (i, j) => i == -1 ? InputAxonType.Create(j) : null);

        var machine = IMachine.Create(network, INetworkFeeder.CreateUniformPrimer());

        var output = machine.RunCollect(2);

        Assert.Equal(actual: output, expected: [[1f], [0f]]);
    }
    [Fact]
    public void TestNeuronCanActivateSelf()
    {
        var network = INetwork.Create(NeuronTypes.OnlyOne.Select(INeuron.Create).ToList(),
                                      outputCount: 1,
                                      UpdateWeightsDelegateExtensions.Empty,
                                      (i, j) => i == -1 ? InputAxonType.Create(j) : AxonBuilders.LengthTwo(i, j));

        var machine = IMachine.Create(network, INetworkFeeder.CreateUniformPrimer());

        var output = machine.RunCollect(3);
        Assert.Equal(actual: output, expected: [[1f], [0f], [1f]]);
    }
    [Fact]
    public void Should_say_how_many_neurons_fired()
    {
        // Schema:
        //     I ━━━(N)━━━┓
        //           ↑    ┃Length=2
        //           ┗━━━━┛
        // t↓   I1       0    
        // 0             0               // charge at t=0
        //     →0                        // axons that deliver
        //               ̲1* δt=2         // charge at end of t=0. * indicates which fire. Underscore means output
        // 1             0               // charge at t=1
        //                               // axons that deliver
        //               0               // charge after delivery + fires. Underscore means output
        // 2             0               // charge at t=2
        //              →0               // axons that deliver
        //               ̲1               // charge after delivery + fires. Underscore means output
        // 3
        // as you can see, the number of fired neurons is [1, 0, 1]
        // 
        var network = INetwork.Create(NeuronTypes.OnlyOne.Select(INeuron.Create).ToList(),
                                      outputCount: 1,
                                      UpdateWeightsDelegateExtensions.Empty,
                                      (i, j) => i == -1 ? InputAxonType.Create(j) : AxonBuilders.LengthTwo(i, j));

        var machine = IMachine.Create(network, INetworkFeeder.CreateUniformPrimer());

        List<int> neuronExcitationCounts = [];
        machine.OnTicked += (sender, e) => neuronExcitationCounts.Add(e.ExcitationCount);
        machine.Run(maxTime: 3);
        Assert.Equal(3, neuronExcitationCounts.Count);
        Assert.Equal([1, 0], neuronExcitationCounts[0..2]);
        // the third one can be either 0 or 1, depending on whether we update the neurons just to get that number
        Assert.Equal(1, neuronExcitationCounts[2]); // current implementation is 1, but 0 would also be okay
    }

    [Fact]
    public void Neuron_with_initial_charge_fires_normally()
    {
        var network = INetwork.Create([INeuron.Create(NeuronTypes.InitiallyCharged, label: "a")],
                                      outputCount: 1,
                                      UpdateWeightsDelegateExtensions.Empty,
                                      (i, j) => i == -1 ? InputAxonType.Create(j, length: 1, [0.1f]) : null);
        var machine = IMachine.Create(network);
        List<int> neuronExcitationCounts = [];
        machine.OnTicked += (sender, e) => neuronExcitationCounts.Add(e.ExcitationCount);
        var outputs = machine.RunCollect(3).Select(o => o[0]).ToArray();

        Assert.Equal([1, 0, 0], neuronExcitationCounts);
        Assert.Equal([1, 0, 0], outputs);
    }

    [Fact]
    public void Neuron_with_charge_retention_fires_again()
    {
        var network = INetwork.Create([INeuron.Create(NeuronTypes.AlwaysOn, label: "a")],
                                      outputCount: 0,
                                      UpdateWeightsDelegateExtensions.Empty,
                                      (i, j) => null);
        var machine = IMachine.Create(network);
        List<int> neuronExcitationCounts = [];
        machine.OnTicked += (sender, e) => neuronExcitationCounts.Add(e.ExcitationCount);
        machine.Run(3);

        Assert.Equal([1, 1, 1], neuronExcitationCounts);
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

        var connections = AxonBuilders.CreateRandom(neuronCount, connectionChance, random);

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
        var randomInitialization = new float[inputCount][];
        for (int i = 0; i < inputCount; i++)
        {
            randomInitialization[i] = [random.NextSingle() < initializationChance ? 1 : 0];
        }

        var network = INetwork.Create(neuronTypes.Select(INeuron.Create).ToList(),
                                      outputCount,
                                      UpdateWeightsDelegateExtensions.Empty,
                                      (i, j) => i == -1 ? (j < randomInitialization.Length ? InputAxonType.Create(j, 1, randomInitialization[j]) : null) : connections[i, j]);

        var machine = IMachine.Create(network, INetworkFeeder.CreateRandomPrimer(random));

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
        , initialCharge: [1]);

        var neuron = new Neuron(INeuron.Create(type, label: "a"));
        var charges = new float[4];
        for (int t = 0; t < charges.Length; t++)
        {
            neuron.Decay(t);
            charges[t] = neuron.EffectiveCharge;
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
            [(maxDt: 2, decay: 0.5f)], // in contrast to the test above, activation is triggered in this one, hence we use that list
        initialCharge: [0]);

        var neuron = new Neuron(INeuron.Create(type, label: "a"));
        // simulate without triggering Decay
        neuron.Receive([1], Machines.AtTime0);
        neuron.Excite(Machines.AtTime0);

        var charges = new IReadOnlyList<float>[4];
        for (int t = 0; t < charges.Length; t++)
        {
            neuron.Decay(t);
            charges[t] = [.. neuron.Charges];
        }
        // these are the charges at the ends of timesteps just before clearance
        // given that the initial axon fires at time 0 and reaches the neuron between time 0 and 1,
        // the charge at the end of time 0 is 1f
        Assert.Equal([[1f], [0.50f], [0.25f], [0.25f]], charges);
    }

    [Fact]
    public void Clock_passed_to_Feedback_should_be_at_end_of_timestep()
    {
        const int maxTime = 3;
        var network = INetwork.Create([new Either<INeuron, IAxonBuilder>(INeuron.Create(INeuronType.NoRetentionNeuronType, label: "a"))], 1, UpdateWeightsDelegateExtensions.Empty, IClock.Create(maxTime));
        var machine = IMachine.Create(network);
        List<int> feedbackTimes = [];
        machine.OnTicked += (sender, e) => feedbackTimes.Add(sender.Clock.Time);

        // Act
        machine.Run();

        Assert.Equal(feedbackTimes, [0, 1, 2]);
    }

    [Fact]
    public void TestThatInputWithSingleWeightIsAcceptedWhenTheNetworkUsesMultipleWeights()
    {
        // the network is one input axon, one hidden neuron and one output neuron, receiving linea recta from the hidden neuron.
        const int INPUT_NEURON = IAxonBuilder.FROM_INPUT;
        const int HIDDEN_NEURON = 0;
        const int OUTPUT_NEURON = 1;
        float[] actual = Array.Empty<float>();

        UpdateWeightsDelegate weightUpdater = (weights, _, _, _, _, startIndex, endIndex) =>
        {
            actual = weights.ToArray();
        };
        IAxonBuilder? getConnections(int from, int to) => (from, to) switch
        {
            (INPUT_NEURON, HIDDEN_NEURON) => InputAxonType.Create(HIDDEN_NEURON, length: 1, [1f]),
            (HIDDEN_NEURON, OUTPUT_NEURON) => new ImmutableAxonBuilder(length: 1, [1f, (float)Math.PI] /*explicitly has two elements*/, HIDDEN_NEURON, OUTPUT_NEURON),
            _ => null
        };
        var network = INetwork.Create(new INeuronType[] { INeuronType.NoRetentionNeuronType, INeuronType.NoRetentionNeuronType }.Select(INeuron.Create).ToList(), outputCount: 1, weightUpdater, getConnections, IClock.Create(maxTime: 2));
        var machine = IMachine.Create(network);
        machine.OnTicked += (sender, e) => e.Feedback = new MockFeedback();

        // Act
        machine.Run();

        Assert.Equal(2, actual.Length);
    }
}
