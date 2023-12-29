using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface IAxonInitialization
{
    public static IAxonInitialization Input => InputAxonInitialization.Instance;
    public static IAxonInitialization Create(int length, float initialWeight, IAxonType axonType)
    {
        return new AxonInitialization(length, initialWeight, axonType);
    }

    public int Length { get; }
    public float InitialWeight { get; }
    public IAxonType AxonType { get; }

    private sealed record AxonInitialization(int Length, float InitialWeight, IAxonType AxonType) : IAxonInitialization;
}
