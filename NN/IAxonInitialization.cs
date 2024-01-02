using JBSnorro.NN.Internals;

namespace JBSnorro.NN;

public interface IAxonInitialization
{
    public static IAxonInitialization Create(int length, IReadOnlyList<float> initialWeight, IAxonType axonType)
    {
        return new AxonInitialization(length, initialWeight, axonType);
    }

    public int Length { get; }
    public IReadOnlyList<float> InitialWeight { get; }
    public IAxonType AxonType { get; }

    private sealed record AxonInitialization(int Length, IReadOnlyList<float> InitialWeight, IAxonType AxonType) : IAxonInitialization;
}
