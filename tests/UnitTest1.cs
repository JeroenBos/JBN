using Xunit;
using Assert = Xunit.Assert;
using JBSnorro.NN;

public class NetworkTests
{
    [Fact]
    public void CreateNetwork()
    {
        var connections = new AxonType?[1, 1];
        var network = new Network(NeuronTypes.OnlyOne,
                                  inputCount: 1,
                                  outputCount: 1,
                                  connections);
    }

    [Fact]
    public void RunUnactivatedNetwork()
    {
        var connections = new AxonType?[1, 1];
        var network = new Network(NeuronTypes.OnlyOne,
                                  inputCount: 1,
                                  outputCount: 1,
                                  connections);

        var machine = new Machine(network);
        var output = machine.Run(1);
        Assert.Equal(output, new float[,] { { 0 } });
    }


    [Fact]
    public void RunActivatedNetworkOfOne()
    {
        var connections = new AxonType?[1, 1] { { null } };
        var network = new Network(NeuronTypes.OnlyOne,
                                  inputCount: 1,
                                  outputCount: 1,
                                  connections);


        var machine = new Machine(network);
        network.Input[0].Activate(machine);

        var output = machine.Run(1);
        Assert.Equal(output, new float[,] { { 1 } });
    }


    [Fact]
    public void TestNeuronDeactivatesAfterActivation()
    {
        var connections = new AxonType?[1, 1] { { null } };
        var network = new Network(NeuronTypes.OnlyOne,
                                  inputCount: 1,
                                  outputCount: 1,
                                  connections);


        var machine = new Machine(network);
        network.Input[0].Activate(machine);

        var output = machine.Run(2);
        Assert.Equal(output, new float[,] { { 1 }, { 0 } });
    }
    [Fact]
    public void TestNeuronCanActivateSelf()
    {
        var connections = new AxonType?[1, 1] { { AxonTypes.LengthTwo } };
        var network = new Network(NeuronTypes.OnlyOne,
                                  inputCount: 1,
                                  outputCount: 1,
                                  connections);


        var machine = new Machine(network);
        network.Input[0].Activate(machine);

        var output = machine.Run(3);
        Assert.Equal(output, new float[,] { { 1 }, { 0 }, { 1 } });
    }
    [Fact]
    public void StressTest()
    {
        var random = new Random(Seed: 0);
        const int neuronCount = 100;
        const int inputCount = 5;
        const int outputCount = 5;
        const int maxTime = 15;
        const float initializationChange = 1f;
        const float connectionChance = 0.5f;

        var connections = new AxonType?[neuronCount, neuronCount];

        var getLength = AxonType.CreateDefault2DGetLength(neuronCount);
        var getInitialWeight = AxonType.CreateRandomWeightInitializer(random);
        for (int i = 0; i < neuronCount; i++)
        {
            for (int j = 0; j < neuronCount; j++)
            {
                if (random.NextSingle() < connectionChance)
                {
                    connections[i, j] = new AxonType(getLength, getInitialWeight);
                }
            }
        }
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
        var randomInitialization = new bool[inputCount];
        for (int i = 0; i < inputCount; i++)
        {
            randomInitialization[i] = random.NextSingle() < initializationChange;
        }

        var network = new Network(neuronTypes,
                                  inputCount,
                                  outputCount,
                                  connections);


        var machine = new Machine(network);
        foreach (var (activate, input) in randomInitialization.Zip(network.Input))
        {
            if (activate)
            {
                network.Input[0].Activate(machine);
            }
        }

        var output = machine.Run(maxTime);
        for (int t = 0; t < maxTime; t++)
        {
            Console.Write("[");
            for (int i = 0; i < outputCount; i++)
            {
                Console.Write(output[t, i].ToString("n2"));
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
        var n = new VariableNeuronType(new[] {
            (2, 0.5f)
         }, new (int, float)[0]);

        var sequence = n.GetNoActivationDecaySequence().Take(4).Select(f => f.ToString("n2")).ToList();
        Assert.Equal(sequence, new[] { "0.50", "0.50", "0.00", "0.00" });
    }

    [Fact]
    public void TestCumulativeDecaySequence()
    {
        var n = new VariableNeuronType(new[] {
            (3, 0.5f)
         }, new (int, float)[0]);

        var sequence = n.GetNoActivationCumulativeDecaySequence().Take(4).Select(f => f.ToString("n2")).ToList();
        Assert.Equal(sequence, new[] { "0.50", "0.25", "0.12", "0.00" });
    }


    [Fact]
    public void TestNeuronInitialChargeDecay()
    {
        var type = new VariableNeuronType(new[] {
            (2, 0.5f)
         }, new (int, float)[0]);

        var neuron = new Neuron(type, 0, initialCharge: 1);
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
        neuron.Activate(Machines.AtTime0);

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

}
