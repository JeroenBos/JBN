namespace JBSnorro.NN.Internals;

/// <summary>
/// Represents the singleton sentinel object <see cref="IAxonBuilder.FromInputLabel"/> 
/// </summary>
internal sealed class InputSingleton
{
    // maybe this type is just equivalent to System.Object?
    public static InputSingleton Instance { get; } = new();
    private InputSingleton() { }
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(Instance, obj);
    }
    public override int GetHashCode()
    {
        return 1;
    }
}
