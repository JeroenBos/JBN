namespace JBSnorro.NN; // not JBSnorro, because JBSnorro.dll already has a type called `Either'2` and would conflict

public sealed class Either<T1, T2> where T1 : notnull where T2 : notnull
{
    public T1? Value1 { get; }
    public T2? Value2 { get; }
    public Either(T1 t)
    {
        this.Value1 = t ?? throw new ArgumentNullException(nameof(t));
    }
    public Either(T2 u)
    {
        this.Value2 = u ?? throw new ArgumentNullException(nameof(u));
    }

    public bool TryGet(out T1 value)
    {
        value = this.Value1!;
        return value is not null;
    }
    public bool TryGet(out T2 value)
    {
        value = this.Value2!;
        return value is not null;
    }
    /// <summary>
    /// Extracts the value, whichever one it is.
    /// </summary>
    /// <returns><see langword="true"/> if this is a <typeparamref name="T1"/>, <see langword="false"/> if this is a <typeparamref name="T2"/>. </returns>
    public bool Get(out T1 value1, out T2 value2)
    {
        value1 = this.Value1!;
        value2 = this.Value2!;
        return value1 is not null;
    }

    public static implicit operator Either<T1, T2>(T1 value) => new(value);
    public static implicit operator Either<T1, T2>(T2 value) => new(value);
}

// also internal to prevent conflicts with JBSnorro.dll
static class EnumerableExtensions
{
    public static IEnumerable<T> Range2D<T>(int start0, int end0, int start1, int end1, Func<int, int, T> selector)
    {
        for (int i = start0; i <= end0; i++)
        {
            for (int j = start1; j <= end1; j++)
            {
                yield return selector(i, j);
            }
        }
    }
}
