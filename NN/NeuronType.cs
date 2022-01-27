namespace JBSnorro.NN;

public sealed class NeuronType
{
    public const int NEVER = int.MinValue + 1;
    /// <summary> Gets the decay of the charge given the times since last recept of charge and last activation. </summary>
    public float GetDecay(int timeSinceLastChargeReceipt, int timeSinceLastActivation)
    {
        if (timeSinceLastChargeReceipt <= 0)
            throw new ArgumentOutOfRangeException(nameof(timeSinceLastChargeReceipt));

        // this NEVER/2 trick effectively puts the max run time at half of int.MaxValue. Fine for now
        if (NEVER / 2 < timeSinceLastActivation && timeSinceLastActivation <= 0)
            throw new ArgumentOutOfRangeException(nameof(timeSinceLastActivation));

        if (timeSinceLastActivation == 1)
            return 0;
        return 1;
    }
}
