namespace JBSnorro.NN.Internals;

internal sealed class Machine : IMachine
{
    private readonly Network network;
    private readonly GetFeedbackDelegate getFeedback;
    private readonly List<Neuron> potentiallyActivatedDuringStep;
    private readonly List<List<Axon>> emits;

    private int maxTime = -1;
    private int t = -1;

    public event OnTickDelegate? OnTick;

    internal int Time => t;

    public Machine(Network network) : this(network, _ => Feedback.Empty) { }
    public Machine(Network network, GetFeedbackDelegate getFeedback)
    {
        this.network = network;
        this.getFeedback = getFeedback;
        this.potentiallyActivatedDuringStep = new List<Neuron>();
        this.emits = new List<List<Axon>> { new() };
    }

    public float[,] Run(int maxTime)
    {
        if (t != -1) throw new InvalidOperationException("This machine has already run");
        if (emits[0].Count != 0) throw new Exception("this.emits[0].Count == 0");

        this.maxTime = maxTime;
        emits.RemoveAt(0);  // if t starts at -1, input neurons with length 1 add to this.emits[1]
        emits.Add(new List<Axon>()); // so we need to skip this.emits[0]

        float[,] output = Extensions.Initialize2DArray(maxTime, network.output.Length, float.NaN);
        // assumes the input axioms have been triggered
        for (t = 0; t < maxTime; t++)
        {
            Tick();

            var latestOutput = CopyOutput(output);
            bool stop = ProcessFeedback(latestOutput);
            if (stop)
                break;

            network.Decay(t + 1);
        }
        return output;
    }
    private void Tick()
    {
        var emittingAxons = emits[0];
        float positiveCumulativeOomph = 0;
        float negativeCumulativeOomph = 0;
        foreach (Axon axon in emittingAxons)
        {
            float w = axon.Weight;
            if (w < 0)
                negativeCumulativeOomph -= w;
            else
                positiveCumulativeOomph += w;
            axon.Emit(this);
        }
        int activationCount = 0;
        foreach (Neuron neuron in potentiallyActivatedDuringStep)
        {
            if (neuron.Charge >= Neuron.threshold)
            {
                activationCount++;
                neuron.Activate(this);
            }
        }

        this.OnTick?.Invoke(this, new OnTickEventArgs(Time: Time, EmittingAxonCount: emittingAxons.Count, PositiveCumulativeOomph: positiveCumulativeOomph, NegativeCumulativeOomph: negativeCumulativeOomph, ActivationCount: activationCount));

        // clean up
        emittingAxons.Clear();
        emits.RemoveAt(0);
        emits.Add(emittingAxons);
        potentiallyActivatedDuringStep.Clear();

        // so what kind of events are there and how do they relate to the integer nature of time?
        // - decay of a neuron
        //   - must be after charge has been outputted
        //   - if it receive charge on time T, let's say also the decay of time T still happens
        //   so decay or a neuron happens just after time T (except at t=0)
        // - activation of a neuron
        //   - happens at the end
        //   - must happen after all axons this time step have fired. if it peeks over the threshold
        //     and dips below again, it will not activate.
        // - registering output
        //   happens at the end (order w.r.t. activation is not relevant)
        // - clear neuron charge
        //   happens at the end after registering output
        // the input axons are fired at time -1, and with length 1 arrive at their nodes in time 0
        // therefore, any initial charge (if any/implemented) should not decay just after time 0
        // - receiving charge
        //   happens in the middle of a timestep

    }
    private float[] CopyOutput(float[,] totalOutput)
    {
        // PERF: use ReadOnlySpan2D<T>
        var latestOutput = network.output;
        for (int i = 0; i < latestOutput.Length; i++)
        {
            totalOutput[this.t, i] = latestOutput[i];
        }
        return latestOutput;
    }
    private bool ProcessFeedback(ReadOnlySpan<float> latestOutput)
    {
        var feedback = getFeedback(latestOutput);
        network.ProcessFeedback(feedback.Dopamine, feedback.Cortisol, Time);
        return feedback.Stop;
    }

    public void AddEmitAction(int time, Axon axon)
    {
        if (maxTime == -1) throw new InvalidOperationException("maxTime == -1");
        if (time >= maxTime && maxTime != -1)
            return;
        int dt = time - this.t;
        if (dt <= 0) throw new ArgumentOutOfRangeException(nameof(time), "dt <= 0");

        while (dt >= emits.Count)
        {
            emits.Add(new List<Axon>());
        }
        emits[dt].Add(axon);
    }
    public void RegisterPotentialActivation(Neuron neuron)
    {
        potentiallyActivatedDuringStep.Add(neuron);
    }
    int IMachine.Time => Time;
}
