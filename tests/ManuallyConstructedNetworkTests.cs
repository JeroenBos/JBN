using JBSnorro.NN;
using JBSnorro.NN.Internals;
using System.Diagnostics;

namespace Tests.JBSnorro.NN;

public class AND
{
    class NetworkFactory : INetworkFactory
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
                    return InputAxonType.Instance;
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

        INetworkFactory factory = new NetworkFactory(INetworkFeeder.CreateDeterministicFeeder(inputSequence));
        return factory.Create(maxTime);
    }
    /// <summary>
    /// Schema:
    ///     I ━━━(N0)━━━━━━━┓              
    ///                    (N2)
    ///     I ━━━(N1)━━━━━━━┛
    /// 
    /// t↓  I     N0        N1         N2     // means that we have Input (I) firings, and one neuron (N1)
    ///          (0)       (0)        (0)     // initial charges
    ///           ┊         ┊          ┊      // means that we didn't receive charge from the previous step (as there was no previous step)
    /// 0   + ━━━(1)  + ━━━(1)      ┈┈(0)     // the + means that the input axon fired. Then the (1) means the cumulative incoming charge results, and →0 means the charge after firing is 0.
    ///           │                           // means the neuron fired its axon (I propose ┈ and ┊ for "not fired")
    /// 1   + ━━━(2→0)                        // means that the Input axon fired, and the (2) is the sum of the previous charge (0, because it fired) and the two incoming axons
    ///           │                    
    /// 2        (1)                          // means the output at time=2 was positive
    /// 
    /// 
    ///     I ━━━(0→1)┈┈┈┈┈┈┈              
    ///                      (0)
    ///     I ━━━(0→1)┈┈┈┈┈┈┈
    ///
    ///     I1        N0    N1    N2
    /// t↓            0     0     0           // charge at t=0
    ///     →0 →1                             // axons that deliver
    ///               1*    1*    0           // charge at end of t=0. * indicates which fire
    /// 1             0     0     0           // charge at t=1
    ///     →0 →1     →2    →2                // axons that deliver
    ///               1*    1*    2*          // charge after delivery
    /// 2                                     // not relevant to test anymore

    
    /// https://en.wikipedia.org/wiki/List_of_Unicode_characters#Box_Drawing
    /// </summary>
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
