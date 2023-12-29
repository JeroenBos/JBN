namespace JBSnorro.NN.Internals;

internal sealed class LimitedClock : IClock
{
    public int MaxTime { get; }
    public int Time { get; private set; }


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
internal sealed class UnlimitedClock : IClock
{
    public int Time { get; private set; }

    public void Increment()
    {
        this.Time++;
    }

    int? IReadOnlyClock.MaxTime => null;
}

public interface IReadOnlyClock
{
    public int Time { get; }
    public int? MaxTime { get; }
}

public interface IClock : IReadOnlyClock
{
    public static IClock Create(int? maxTime)
    {
        return maxTime is null ? new UnlimitedClock() : new LimitedClock(maxTime.Value);
    }
    public void Increment();


    internal IEnumerable<int> Ticks
    {
        get
        {
            for (; this.MaxTime is null || Time < MaxTime.Value; this.Increment())
            {
                yield return Time;
            }
        }
    }
}
