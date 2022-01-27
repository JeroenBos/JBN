namespace JBSnorro.NN;

public sealed class Feedback
{
    public static Feedback Empty { get; } = new Feedback();

    public float Dopamine { get; init; }
    public float Cortisol { get; init; }
    public bool Stop { get; init; }
}

public delegate Feedback GetFeedbackDelegate(float[] latestOutput);
