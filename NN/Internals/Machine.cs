namespace JBSnorro.NN.Internals;

internal sealed class Machine : IMachine
{
    private readonly INetwork network;
    private readonly GetFeedbackDelegate getFeedback;
    private readonly List<Neuron> potentiallyExcitedDuringStep;
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
    /// Triggered at the end of a time step, with the event args containing what happened in this time step.
    /// </summary>
    public event OnTickDelegate? OnTicked;

    public Machine(INetwork network, GetFeedbackDelegate getFeedback)
    {
        this.network = network;
        this.clock = network.MutableClock;
        this.getFeedback = getFeedback;
        this.potentiallyExcitedDuringStep = new List<Neuron>(); // TODO: make it a HashSet?
        this.emits = [[]];
    }

    /// <summary>
    /// Order of events:
    /// - if clock is not started, deliver input axon firings
    /// - while time is below maxTime (exclusive)
    ///   - neurons update:
    ///     - those reached threshold fire and go into refractory state
    ///     - others' charge decay
    ///   - axons firings are delivered
    ///   - output is measured
    /// 
    /// </summary>
    public float[] Run(int? maxTime = null)
    {
        if (clock.Time != IReadOnlyClock.UNSTARTED) throw new InvalidOperationException("This machine has already run");
        if (clock.MaxTime.HasValue && clock.MaxTime < maxTime) throw new ArgumentException("maxTime > this.Clock.MaxTime", nameof(maxTime));
        if (maxTime is null && clock.MaxTime is null) throw new ArgumentException("Neither the clock nor the specified argument has a max time", nameof(maxTime));

        this.DeliverFiredAxons(new OnTickEventArgs(IReadOnlyClock.UNSTARTED));

        float[] output = network.Output;
        foreach (var time in maxTime == null ? clock.Ticks : clock.Ticks.TakeWhile(time => time < maxTime)) // .Prepend(IReadOnlyClock.UNSTARTED)
        {
            var e = new OnTickEventArgs(time);

            this.UpdateNeurons(e);
            this.DeliverFiredAxons(e);

            e.Output = output = network.Output;
            bool stop = ProcessFeedback(output);

            this.OnTicked?.Invoke(this, e);

            if (stop)
            {
                break;
            }
        }
        return output;
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
        int excitationCount = 0;
        foreach (Neuron neuron in potentiallyExcitedDuringStep)
        {
            if (neuron.Charge >= Neuron.threshold)
            {
                excitationCount++;
                neuron.Excite(this);
            }
        }
        e.ExcitationCount = excitationCount;

        // end of time step:
        network.Decay();

        // clean up
        potentiallyExcitedDuringStep.Clear();
        emits.RemoveAt(0);
    }
    private bool ProcessFeedback(ReadOnlySpan<float> latestOutput)
    {
        var feedback = this.getFeedback(latestOutput, this.Clock);
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
    public void RegisterPotentialExcitation(Neuron neuron)
    {
        potentiallyExcitedDuringStep.Add(neuron);
    }
    public IReadOnlyClock Clock => network.Clock;
    public float[] Output => network.Output;
}
