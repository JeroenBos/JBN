global using static Contracts;
global using static Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public static class Extensions
{
    public static IEnumerable<T> Fold<T>(this IEnumerable<T> sequence, Func<T, T, T> apply, T initial = default!)
    {
        T current = initial;
        foreach (T element in sequence)
        {
            current = apply(current, element);
            yield return current;
        }
    }
}


class Contracts
{
    [Conditional("DEBUG"), DebuggerHidden]
    public static void Assert([DoesNotReturnIf(false)] bool condition, [CallerArgumentExpression("condition")] string message = "")
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }
}


class Helpers
{
    internal const int MAX_AXON_LENGTH = 100;
    /// <summary> If initialized to this value, it works together with <see cref="Helpers.IsNever(int)"> 
    /// and being subtracted from <see cref="decayUpdatedTime">. </summary>
    internal const int NEVER = int.MaxValue - MAX_AXON_LENGTH - 1;
    /// <summary> Gets whether any time minus a time that could contain <see cref="NEVER"/> contained never. </summary>
    internal static bool IsNever(int t)
    {
        // the int.MinValue/2 trick effectively puts the max run time at half of int.MaxValue. Fine for now
        return t < int.MinValue / 2;
    }
}
