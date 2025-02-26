namespace JBSnorro.NN;


/// <summary>
/// This exists mainly to communicate for axons about which neurons they connect to.
/// </summary>
public interface INeuron
{
    INeuronType Type { get; }
    /// <summary>
    /// An object the user can pass by reference to identify neurons by.
    /// The default object equality comparison is used, that is <see cref="object.Equals(object?)"/> 
    /// </summary>
    object Label { get; }

    public static INeuron Create(INeuronType type, int index) => Create(type, (object)index);
    public static INeuron Create(INeuronType type, object label)
    {
        return new Implementation(type ?? throw new ArgumentNullException(nameof(type)), label ?? throw new ArgumentNullException(nameof(label)));
    }

    private sealed record Implementation(INeuronType Type, object Label) : INeuron { }
}
