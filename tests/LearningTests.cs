using JBSnorro.NN;
using JBSnorro.NN.Internals;
using Assert = Xunit.Assert;

namespace Tests.JBSnorro.NN;

public class LearningTests
{
    sealed class WeightChangingAxonType(Action<float[]> changeWeights) : IAxonType
    {
        private readonly Action<float[]> changeWeights = changeWeights;
        public int Length => 1;
        public IReadOnlyList<float> InitialWeights => [1f];

        public void UpdateWeights(float[] currentWeights, int timeSinceLastExcitation, float averageTimeBetweenExcitations, int excitationCount, IFeedback feedback)
        {
            this.changeWeights(currentWeights);
        }
    }
    /// <summary>
    /// 
    ///     (N0)━━━━(N1)
    /// 
    /// </summary>
    [Fact]
    public void Network_can_change_to_output_zero()
    {
        static IAxonType? GetAxonConnection(int neuronFromIndex, int neuronToIndex)
        {
            switch ((neuronFromIndex, neuronToIndex))
            {
                case (0, 1): return new WeightChangingAxonType(currentWeights => currentWeights[0] -= 0.05f);
                default: return null;
            }
        }

        var network = INetwork.Create([NeuronTypes.AlwaysOn, NeuronTypes.NoRetention], outputCount: 1, GetAxonConnection, IClock.Create(100));
        var machine = IMachine.Create(network);
        machine.OnTicked += (sender, e) =>
        {
            if (e.Time != 0) // because then output is 0
            {
                e.Feedback = MockFeedback.Instance; // triggers calling IAxonType.UpdateWeights
                e.Stop = float.Abs(e.Output[0]) < 0.1;
            }
        };
        machine.Run();

        Assert.True(10 < network.Clock.Time);
        Assert.True(network.Clock.Time < 50);
    }
}
