namespace JBSnorro.NN;

public interface IReadOnlyClock
{
    /// <summary>
    /// The time of the clock before it has been started.
    /// </summary>
    public const int UNSTARTED = -1;

    public int Time { get; }
    public int? MaxTime { get; }
}
