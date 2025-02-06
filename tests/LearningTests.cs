using JBSnorro.NN;
using JBSnorro.NN.Internals;
using Assert = Xunit.Assert;

namespace Tests.JBSnorro.NN;

public class LearningTests
{
    /// <summary>
    /// 
    ///     (N0)━━━━(N1)
    /// 
    /// </summary>
    [Fact]
    public void Network_can_change_to_output_zero()
    {
        static IAxonBuilder? getAxonConnection(int neuronFromIndex, int neuronToIndex)
        {
            switch ((neuronFromIndex, neuronToIndex))
            {
                case (0, 1): return new WeightChangingAxonType<MockFeedback>((currentWeights, _feedback) => currentWeights[0] -= 0.05f, 0, 1);
                default: return null;
            }
        }

        var network = INetwork.Create([INeuron.Create(NeuronTypes.AlwaysOn, label: null), INeuron.Create(NeuronTypes.NoRetention, label: null)], outputCount: 1, getAxonConnection, IClock.Create(100));
        var machine = IMachine.Create(network);
        machine.OnTicked += (sender, e) =>
        {
            if (e.Time != 0) // because then output is 0
            {
                e.Feedback = MockFeedback.Instance; // triggers updateWeights
                e.Stop = float.Abs(e.Output[0]) < 0.1;
            }
        };
        machine.Run();

        Assert.True(10 < network.Clock.Time);
        Assert.True(network.Clock.Time < 50);
    }
    /// <summary>
    /// 
    ///     (N0)━━━━(N1)
    /// 
    /// </summary>
    [Fact]
    public void Network_can_mimick_value()
    {
        static IAxonBuilder? getAxonConnection(int neuronFromIndex, int neuronToIndex)
        {
            switch ((neuronFromIndex, neuronToIndex))
            {
                case (0, 1):
                    return new WeightChangingAxonType<DirectionalFeedback>((currentWeights, feedback) =>
                    {
                        if (feedback == DirectionalFeedback.TooHigh)
                        {
                            currentWeights[0] -= 0.5f;
                        }
                        else
                        {
                            currentWeights[0] += 0.5f;
                        }
                    }, 0, 1);
                default: return null;
            }
        }

        var network = INetwork.Create([INeuron.Create(NeuronTypes.AlwaysOn, label: null), INeuron.Create(NeuronTypes.NoRetention, label: null)], outputCount: 1, getAxonConnection, IClock.Create(100));
        var machine = IMachine.Create(network);

        int aimAchieved = 0;
        machine.OnTicked += (sender, e) =>
        {
            int aim = aimAchieved switch
            {
                0 => 1,
                1 => -1,
                2 => 1,
                _ => -2,
            };

            e.Feedback = e.Output[0] > aim ? DirectionalFeedback.TooHigh : DirectionalFeedback.TooLow;
            if (float.Abs(e.Output[0] - aim) < 0.1)
            {
                aimAchieved++;
                if (aimAchieved == 4)
                    e.Stop = true;
            }
        };
        var outputs = machine.RunCollect();

        Assert.True(10 < network.Clock.Time);
        Assert.True(network.Clock.Time < 50);
    }
}
