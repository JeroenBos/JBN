using JBSnorro.NN;
using JBSnorro.NN.Internals;
using System.Diagnostics;

namespace Tests.JBSnorro.NN;

public class AND
{
    class NetworkFactory : INetworkFactory<IFeedback>
    {
        public IReadOnlyList<INeuronType> NeuronTypes { get; } = Enumerable.Repeat(INeuronType.NoRetentionNeuronType, /*neuron count: */3).ToArray();
        public int OutputCount => 1;
        public INetworkFeeder InputFeeder { get; }

        public NetworkFactory(INetworkFeeder inputPrimer)
        {
            this.InputFeeder = inputPrimer;
        }

        public IAxonType? GetAxonConnection(int neuronFromIndex, int neuronToIndex)
        {
            switch ((neuronFromIndex, neuronToIndex))
            {
                case (IAxonType.FROM_INPUT, 0):
                case (IAxonType.FROM_INPUT, 1):
                    return IAxonType.Input;
                case (0, 2):
                case (1, 2):
                    return IAxonType.CreateImmutable(length: 1, new float[] { 0.5f });
                case (0, 0):
                case (1, 1):
                case (2, 2):
                case (2, 1):
                case (2, 0):
                case (0, 1):
                case (1, 0):
                case (IAxonType.FROM_INPUT, 2):
                    return null;
                default: throw new UnreachableException();
            }
        }
    }
    private (IMachine Machine, INetwork Network) Construct(bool input1, bool input2, int maxTime = 2)
    {
        var inputSequence = new[] { new bool[] { input1, input2 } };

        INetworkFactory<IFeedback> factory = new NetworkFactory(INetworkFeeder.CreateDeterministicFeeder(inputSequence));
        return factory.Create(maxTime);
    }
    [Fact]
    public void True_and_true_gives_true()
    {
        var machine = Construct(true, true).Machine;
        var output = machine.Run();

        Assert.True(output[0] > 0);
    }
    [Fact]
    public void False_and_true_gives_false()
    {
        var machine = Construct(false, true).Machine;
        var output = machine.Run();

        Assert.True(output[0] == 0.5);
    }
    [Fact]
    public void True_and_false_gives_false()
    {
        var machine = Construct(true, false).Machine;
        var output = machine.Run();

        Assert.True(output[0] == 0.5);
    }
    [Fact]
    public void False_and_false_gives_false()
    {
        var machine = Construct(false, false).Machine;
        var output = machine.Run();

        Assert.True(output[0] == 0);
    }
}
