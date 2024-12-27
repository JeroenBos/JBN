namespace JBSnorro.NN.Internals;

[DebuggerDisplay("Clock({Time}/{MaxTime})")]
internal sealed class LimitedClock : IClock
{
    public int MaxTime { get; }
    public int Time { get; private set; } = IReadOnlyClock.UNSTARTED;


    public LimitedClock(int maxTime)
    {
        if (maxTime <= 0) throw new ArgumentOutOfRangeException(nameof(maxTime));

        this.MaxTime = maxTime;
    }

    public void Increment()
    {
        if (this.Time == this.MaxTime)
        {
            throw new InvalidOperationException("max time reached");
        }

        this.Time++;
    }

    int? IReadOnlyClock.MaxTime => this.MaxTime;

}

[DebuggerDisplay("Clock({Time}/∞)")]
internal sealed class UnlimitedClock : IClock
{
    public int Time { get; private set; } = IReadOnlyClock.UNSTARTED;

    public void Increment()
    {
        this.Time++;
    }

    int? IReadOnlyClock.MaxTime => null;
}

internal interface IClock : IReadOnlyClock
{
    public static IClock Create(int? maxTime)
    {
        return maxTime is null ? new UnlimitedClock() : new LimitedClock(maxTime.Value);
    }


    public void Increment();
    public void Start()
    {
        if (this.Time != IReadOnlyClock.UNSTARTED)
        {
            throw new InvalidOperationException("Clock has already been started");
        }

        // go from UNSTARTED to 0:
        this.Increment();
    }


    internal IEnumerable<int> Ticks
    {
        get
        {
            for (this.Start(); this.MaxTime is null || Time < MaxTime.Value; this.Increment())
            {
                yield return Time;
            }
        }
    }
}
