namespace JBSnorro.NN;


/// <summary>
/// This exists mainly to communicate for axons about which neurons they connect to.
/// </summary>
public interface INeuron
{
    INeuronType Type { get; }
    /// <summary>
    /// An object the user can pass by reference to identify neurons by.
    /// </summary>
    object? Label { get; }
}
