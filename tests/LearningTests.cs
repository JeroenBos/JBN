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
        UpdateWeightsDelegate weightUpdater = (weights, _, _, _, _, startIndex, endIndex) =>
        {
            // if ((int)startIndex == 0 && (int)endIndex == 1) {
            weights[0] -= 0.05f;
            // }
        };
        static IAxonBuilder? getAxonConnection(int neuronFromIndex, int neuronToIndex)
        {
            switch ((neuronFromIndex, neuronToIndex))
            {
                case (0, 1): return AxonBuilders.Default(0, 1);
                default: return null;
            }
        }

        var network = INetwork.Create(new INeuronType[] { NeuronTypes.AlwaysOn, NeuronTypes.NoRetention }.Select(INeuron.Create).ToList(), outputCount: 1, weightUpdater, getAxonConnection, IClock.Create(100));
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
        UpdateWeightsDelegate weightUpdater = (weights, _, _, _, feedback, _, _) =>
        {
            if (feedback == DirectionalFeedback.TooHigh)
            {
                weights[0] -= 0.5f;
            }
            else
            {
                weights[0] += 0.5f;
            }
        };
        static IAxonBuilder? getAxonConnection(int neuronFromIndex, int neuronToIndex)
        {
            switch ((neuronFromIndex, neuronToIndex))
            {
                case (0, 1):
                    return AxonBuilders.Default(0, 1);
                default: return null;
            }
        }

        var network = INetwork.Create(new INeuronType[] { NeuronTypes.AlwaysOn, NeuronTypes.NoRetention }.Select(INeuron.Create).ToList(), outputCount: 1, weightUpdater, getAxonConnection, IClock.Create(100));
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
