namespace JBSnorro.NN;

/// <summary>
/// Passing-through data that axons will receive after a step for updating update themselves.
/// It is obtained by setting on <see cref="OnTickEventArgs.Feedback"/> and passed opaquely to <see cref="IAxonBuilder.UpdateWeights"/> .
/// </summary>
public interface IFeedback
{
}
