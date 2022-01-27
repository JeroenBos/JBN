namespace JBSnorro.NN;

public interface INeuronType
{
    internal const int NEVER = int.MinValue + 1;
    float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastActivation);
}
public sealed class RetentionOfOneNeuronType : INeuronType
{
    /// <summary> Gets the decay of the charge given the times since last recept of charge and last activation. </summary>
    public float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastActivation)
    {
        if (timeSinceLastChargeReceipt <= 0)
            throw new ArgumentOutOfRangeException(nameof(timeSinceLastChargeReceipt));

        // this NEVER/2 trick effectively puts the max run time at half of int.MaxValue. Fine for now
        if (INeuronType.NEVER / 2 < timeSinceLastActivation && timeSinceLastActivation <= 0)
            throw new ArgumentOutOfRangeException(nameof(timeSinceLastActivation));

        if (timeSinceLastActivation == 1)
            return 0;
        return 1;
    }
}
