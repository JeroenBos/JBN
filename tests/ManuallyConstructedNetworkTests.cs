using JBSnorro.NN;
using JBSnorro.NN.Internals;
using System.Diagnostics;

namespace Tests.JBSnorro.NN;

// https://en.wikipedia.org/wiki/List_of_Unicode_characters#Box_Drawing

public class AND
{
    /// <summary>
    /// Schema:
    ///     I ━━━(N0)━━━━━━━┓              
    ///                    (N2)
    ///     I ━━━(N1)━━━━━━━┛
    /// </summary>

    public static class BinaryOPNetworkFactory
    {
        public static (IMachine Machine, INetwork Network) Construct(float weight_N01_N2, IEnumerable<IReadOnlyList<bool>> feeds, int maxTime = 2)
        {
            return Construct([weight_N01_N2, weight_N01_N2], feeds, maxTime);
        }
        public static (IMachine Machine, INetwork Network) Construct(IReadOnlyList<float> weights_to_N2, IEnumerable<IReadOnlyList<bool>> feeds, int maxTime = 2)
        {
            return INetwork.Create(
                neuronTypes: Enumerable.Repeat(INeuronType.NoRetentionNeuronType, /*neuron count: */3).ToArray(),
                outputCount: 1,
                getAxon: GetAxonConnection,
                feeder: INetworkFeeder.CreateDeterministicFeeder(feeds),
                maxTime);

            IAxonBuilder? GetAxonConnection(int neuronFromIndex, int neuronToIndex)
            {
                switch ((neuronFromIndex, neuronToIndex))
                {
                    case (IAxonBuilder.FROM_INPUT, 0):
                    case (IAxonBuilder.FROM_INPUT, 1):
                        return InputAxonType.Instance;
                    case (0, 2):
                    case (1, 2):
                        return IAxonBuilder.CreateImmutable(length: 1, [weights_to_N2[neuronFromIndex]]);
                    case (IAxonBuilder.FROM_INPUT, 2):
                    case (0, 0):
                    case (1, 1):
                    case (2, 2):
                    case (2, 1):
                    case (2, 0):
                    case (0, 1):
                    case (1, 0):
                        return null;
                    default: throw new UnreachableException();
                }
            }
        }
    }
    /// <summary>
    /// Schema:
    ///     I ━━━(N0)━━━━━━━┓              
    ///                    (N2)
    ///     I ━━━(N1)━━━━━━━┛
    ///
    /// t↓   I1       N0    N1    N2
    /// 0             0     0     0           // charge at t=0
    ///     →0 →1                             // axons that deliver
    ///               1*    1*    ̲0           // charge at end of t=0. * indicates which fire. Underscore means output
    /// 1             0     0     0           // charge at t=1
    ///               →2    →2                // axons that deliver
    ///               0     0     ̲2           // charge after delivery + fires. Underscore means output
    /// 2                                     // not relevant to test anymore
    /// </summary>
    [Fact]
    public void True_and_true_gives_true()
    {
        var machine = BinaryOPNetworkFactory.Construct(1f, [[true, true]]).Machine;
        var output = machine.Run();

        Assert.Equal(2, output[0]);
    }
    [Fact]
    public void False_and_true_gives_false()
    {
        var machine = BinaryOPNetworkFactory.Construct(1f, [[false, true]]).Machine;
        var output = machine.Run();

        Assert.Equal(1, output[0]);
    }
    [Fact]
    public void True_and_false_gives_false()
    {
        var machine = BinaryOPNetworkFactory.Construct(1f, [[true, false]]).Machine;
        var output = machine.Run();

        Assert.Equal(1, output[0]);
    }
    [Fact]
    public void False_and_false_gives_false()
    {
        var machine = BinaryOPNetworkFactory.Construct(1f, [[false, false]]).Machine;
        var output = machine.Run();

        Assert.Equal(0, output[0]);
    }
}

public class XOR
{
    /// <summary>
    /// Schema:
    ///     I ━━━(N0)━━━━━━━┓              
    ///                    (N2)
    ///     I ━━━(N1)━━━━━━━┛
    /// </summary>
    [Fact]
    public void True_and_true_gives_true()
    {
        var machine = AND.BinaryOPNetworkFactory.Construct([1f, -1f], [[true, true]]).Machine;
        var output = machine.Run();

        Assert.Equal(0, output[0]);
    }
    [Fact]
    public void True_and_false_gives_false()
    {
        var machine = AND.BinaryOPNetworkFactory.Construct([1f, -1f], [[true, false]]).Machine;
        var output = machine.Run();

        Assert.Equal(1, output[0]);
    }
    [Fact]
    public void False_and_true_gives_false()
    {
        var machine = AND.BinaryOPNetworkFactory.Construct([1f, -1f], [[false, true]]).Machine;
        var output = machine.Run();

        Assert.Equal(-1, output[0]); // a NOT operator should technically be insert
    }

    [Fact]
    public void False_and_false_gives_false()
    {
        var machine = AND.BinaryOPNetworkFactory.Construct([1f, -1f], [[false, false]]).Machine;
        var output = machine.Run();

        Assert.Equal(0, output[0]);
    }
}


public class NOT
{
    /// <summary>
    /// Schema:
    ///        -    
    ///     I ━━━(N1)
    ///           ↑+
    ///          (N0)
    ///          
    /// t↓   I1       N0    N1
    /// 0             1     0                // charge at t=1
    ///               no                     // axons that deliver. no → because neurons don't fire before t=0
    ///               1*    ̲0                // charge at end of t=1. * indicates which fire. Underscore means output
    /// 1             1     0                // charge at t=1
    ///               →1                     // axons that deliver
    ///               1*    ̲1*               // charge at end of t=1. * indicates which fire. Underscore means output
    /// 2             1     0                // charge at t=2
    ///      →1       →1                     // axons that deliver
    ///               1*    ̲0                // charge after delivery + fires. Underscore means output
    /// 3             1     0                // charge at t=3
    ///      →1       →1                     // axons that deliver
    ///               1*    ̲0                // charge after delivery + fires. Underscore means output
    /// 4             1     0                // charge at t=4
    ///               →1                     // axons that deliver
    ///               1*    ̲1*               // charge at end of t=4. * indicates which fire. Underscore means output
    /// 5
    /// </summary>
    [Fact]
    public void Input_sequence_FTTF_is_negated_by_NOT_operator()
    {
        var network = INetwork.Create([INeuronType.AlwaysOn, INeuronType.NoRetentionNeuronType], 1, GetAxonConnection, IClock.Create(5));
        var machine = IMachine.Create(network, INetworkFeeder.CreateDeterministicFeeder([[false], [false], [true], [true], [false]]));

        var output = machine.RunCollect().Select(o => o[0]).ToArray();

        // t=0 is different because firing from AlwaysOn didn't arrive (because it didn't fire at t=-1 to arrive at t=0)
        Assert.Equal([0, 1, 0, 0, 1], output);
    }
    private static IAxonBuilder? GetAxonConnection(int neuronFromIndex, int neuronToIndex)
    {
        switch ((neuronFromIndex, neuronToIndex))
        {
            case (IAxonBuilder.FROM_INPUT, 1): return InputAxonType.Create([-1f]);
            case (0, 1): return IAxonBuilder.CreateImmutable(length: 1, [1f]);
            default: return null;
        }
    }
}
