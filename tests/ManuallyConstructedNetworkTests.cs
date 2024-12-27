﻿using JBSnorro.NN;
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
    
    class BinaryOPNetworkFactory : INetworkFactory
    {
        public static (IMachine Machine, INetwork Network) Construct(float weight_N01_N2, IEnumerable<IReadOnlyList<bool>> feeds, int maxTime = 2)
        {
            INetworkFactory factory = new BinaryOPNetworkFactory(INetworkFeeder.CreateDeterministicFeeder(feeds), weight_N01_N2);
            return factory.Create(maxTime);
        }

        public IReadOnlyList<INeuronType> NeuronTypes { get; } = Enumerable.Repeat(INeuronType.NoRetentionNeuronType, /*neuron count: */3).ToArray();
        public int OutputCount => 1;
        public INetworkFeeder InputFeeder { get; }
        private readonly float weight_N01_N2;

        public BinaryOPNetworkFactory(INetworkFeeder inputPrimer, float weight_N01_N2)
        {
            this.InputFeeder = inputPrimer;
            this.weight_N01_N2 = weight_N01_N2;
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
                    return IAxonType.CreateImmutable(length: 1, [weight_N01_N2]);
                case (IAxonType.FROM_INPUT, 2):
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
