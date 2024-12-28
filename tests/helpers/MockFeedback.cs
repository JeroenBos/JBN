using JBSnorro.NN;

class MockFeedback : IFeedback
{
    public static MockFeedback Instance { get; } = new MockFeedback();
}
