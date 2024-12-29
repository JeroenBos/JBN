using JBSnorro.NN;

class MockFeedback : IFeedback
{
    public static MockFeedback Instance { get; } = new MockFeedback();
}

sealed class DirectionalFeedback : IFeedback
{
    private DirectionalFeedback() { }
    public static DirectionalFeedback TooLow { get; } = new();
    public static DirectionalFeedback TooHigh { get; } = new();
}
