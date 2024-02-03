namespace JBSnorro.NN.Internals;

internal sealed class Machine : IMachine
{
    private readonly INetwork network;
    private readonly GetFeedbackDelegate getFeedback;
    private readonly List<Neuron> potentiallyActivatedDuringStep;
    /// <summary>
    /// A list of axons to fire per time step.
    /// If the time now is t_0, then <see cref="emits"/>[t_0] + δt represents all axons that will deliver δt timesteps in the future.
    /// </summary>
    /// <remarks>
    /// During a time step the first entry is <see langword="null"/> to bring to light bugs, as it's not allowed for an axon to trigger a neuron instantly.
    /// Before the machine has run, all initial input stimuli are just in the first entry: all those are delivered on the first tick. 
    /// The point is that there is no need for a special extra list at the beginning for the pre-run input.
    /// </remarks>
    private readonly List<List<Axon>> emits;
    private readonly IClock clock;

    /// <summary>
    /// Triggered at the end of a time step, with the event args containing what happened in this time step
    /// </summary>
    public event OnTickDelegate? OnTicked;

    public Machine(INetwork network, GetFeedbackDelegate getFeedback)
    {
        this.network = network;
        this.clock = network.MutableClock;
        this.getFeedback = getFeedback;
        this.potentiallyActivatedDuringStep = new List<Neuron>();
        this.emits = new List<List<Axon>> { new() };
    }

    /// <summary>
    /// Order of events:
    /// - time ticks (starts at -1 so the first time observed is 0)
    /// - if time is maxTime -> abort (meaning maxTime is exclusive)
    /// - axons firings are delivered
    /// - output is measured
    /// - neurons update:
    ///   - those reached threshold fire and go into refractory state
    ///   - others' charge decay
    /// </summary>
    public float[] Run(int? maxTime = null)
    {
        if (clock.Time != IReadOnlyClock.UNSTARTED) throw new InvalidOperationException("This machine has already run");
        if (clock.MaxTime.HasValue && clock.MaxTime < maxTime) throw new ArgumentException("maxTime > this.Clock.MaxTime", nameof(maxTime));
        if (maxTime is null && clock.MaxTime is null) throw new ArgumentException("Neither the clock nor the specified argument has a max time", nameof(maxTime));

        float[] output = network.Output;
        foreach (var time in maxTime == null ? clock.Ticks : clock.Ticks.TakeWhile(time => time < maxTime))
        {
            var e = new OnTickEventArgs { Time = time, Output = output };

            this.DeliverFiredAxons(e);

            e.Output = output = network.Output;
            bool stop = ProcessFeedback(output);

            this.UpdateNeurons(e);
            this.InvokeOnTicked(e, stop);

            if (stop)
            {
                break;
            }
        }
        return output;
    }
    private void InvokeOnTicked(OnTickEventArgs e, bool feedbackStops)
    {
        bool stopping = feedbackStops && this.Clock.Time + 1 != this.Clock.MaxTime;
        this.OnTicked?.Invoke(this, e); // ActivationCount: activationCount));
    }
    private void DeliverFiredAxons(OnTickEventArgs e)
    {
        var emittingAxons = emits[0];
        emits[0] = null!; // to be alerted (through a NullRefException) when there's a bug

        foreach (Axon axon in emittingAxons)
        {
            axon.Emit(this);
        }

        e.EmittingAxonCount = emittingAxons.Count;

        // optimization to reuse list:
        emittingAxons.Clear();
        emits.Add(emittingAxons);
        // emits[0] is popped after the neurons have been updated
    }
    private void UpdateNeurons(OnTickEventArgs e)
    {
        int activationCount = 0;
        foreach (Neuron neuron in potentiallyActivatedDuringStep)
        {
            if (neuron.Charge >= Neuron.threshold)
            {
                activationCount++;
                neuron.Excite(this);
            }
        }
        e.ActivationCount = activationCount;

        // end of time step:
        network.Decay();

        // clean up
        potentiallyActivatedDuringStep.Clear();
        emits.RemoveAt(0);
    }
    private bool ProcessFeedback(ReadOnlySpan<float> latestOutput)
    {
        var feedback = getFeedback(latestOutput, this.Clock);
        if (feedback is null)
            return false;

        network.Process(feedback);
        return feedback.Stop;
    }

    public void AddEmitAction(int deliveryTime, Axon axon)
    {
        if (deliveryTime < 0) throw new ArgumentOutOfRangeException(nameof(deliveryTime));
        if (deliveryTime >= this.Clock.MaxTime)
        {
            return;
        }

        int dt = deliveryTime - (this.Clock.Time == IReadOnlyClock.UNSTARTED ? 0 : this.Clock.Time);

        if (dt == 0 && this.Clock.Time != IReadOnlyClock.UNSTARTED) throw new ArgumentOutOfRangeException(nameof(deliveryTime), "Delivery instantaneous");
        if (dt < 0) throw new ArgumentOutOfRangeException(nameof(deliveryTime), "Delivery in the past");

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
    public IReadOnlyClock Clock => network.Clock;
    public float[] Output => network.Output;
}
