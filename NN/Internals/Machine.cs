namespace JBSnorro.NN.Internals;

/// <inheritdoc/>
internal sealed class Machine : IMachine
{
    public INetwork Network { get; }
    private readonly List<Neuron> potentiallyExcitedDuringStep; // Not necessary to be a HashSet: Neuron.Excite(..) doesn't do anything if called again on the same timestep.
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

    public Machine(INetwork network)
    {
        this.Network = network;
        this.clock = network.MutableClock;
        this.emits = [[]];
        // they're all potentially excited because the initial charge is not known
        this.potentiallyExcitedDuringStep = [..this.Network.Neurons];
    }

    /// <summary>
    /// Order of events:
    /// - if clock is not started, deliver input axon firings
    /// - while time is below maxTime (exclusive)
    ///   - axons firings are delivered
    ///   - output is measured
    ///   - neurons update:
    ///     - those reached threshold fire and go into refractory state
    ///     - others' charge decay
    /// </summary>
    public float[] Run(int? maxTime = null)
    {
        if (clock.Time != IReadOnlyClock.UNSTARTED) throw new InvalidOperationException("This machine has already run");
        if (clock.MaxTime.HasValue && clock.MaxTime < maxTime) throw new ArgumentException("maxTime > this.Clock.MaxTime", nameof(maxTime));
        if (maxTime is null && clock.MaxTime is null) throw new ArgumentException("Neither the clock nor the specified argument has a max time", nameof(maxTime));

        // You might want the neurons to fire at t=-1, but consider these situations:
        // 1) an always-charged neuron: you expect its axons (assuming L=1) to always deliver, even on t=0
        // 2) an initially-charged neuron decaying immediately: you expect it to excite in t=0
        // In 1) you want `UpdateNeurons(new (IReadOnlyClock.UNSTARTED))` but in 2) you do not.
        // Given that it's unsolvable, if you want initial charge to take effect, shift everything on timestep forward, and discard outputs from t=0.

        float[] output = this.Network.Output;
        foreach (var time in maxTime == null ? clock.Ticks : clock.Ticks.TakeWhile(time => time < maxTime))
        {
            var e = new OnTickEventArgs(time, this.Network.Inputs.Count);

            // although you might want the neurons to fire at the beginning, then axons would have to fire with dt=0
            this.DeliverFiredAxons(e);

            e.Output = output = this.Network.Output;

            this.UpdateNeurons(e);

            this.OnTicked?.Invoke(this, e);

            if (e.Stop)
            {
                break;
            }
            if (e.Feedback is not null)
            {
                this.Network.Process(e.Feedback);
            }

            // clean up
            emits.RemoveAt(0);
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
        foreach (Neuron neuron in potentiallyExcitedDuringStep)
        {
            if (neuron.Charge >= Neuron.threshold)
            {
                if (neuron.Excite(this))
                {
                    e.ExcitationCount++;
                }
            }
        }

        potentiallyExcitedDuringStep.Clear(); // can be appended to in network.Decay()
        this.Network.Decay(this); // could theoretically add to emits, so must be before emits.RemoteAt(0)
    }

    public void Excite(int inputAxonIndex)
    {
        this.Network.Inputs[inputAxonIndex].Excite(this);
    }
    /// <summary>
    /// Sets the specified axon to emit charge at the specified time.
    /// </summary>
    /// <param name="timeOfDelivery">The time the emit is to be delivered at the end of its axon. </param>
    public void AddEmitAction(int deliveryTime, Axon axon)
    {
        if (deliveryTime < 0) throw new ArgumentOutOfRangeException(nameof(deliveryTime));
        if (deliveryTime > this.Clock.MaxTime)
        {
            return;
        }

        int dt = deliveryTime - Math.Max(this.Clock.Time, 0);

        if (dt == 0 && this.Clock.Time != IReadOnlyClock.UNSTARTED) throw new ArgumentOutOfRangeException(nameof(deliveryTime), "Delivery instantaneous");
        if (dt < 0) throw new ArgumentOutOfRangeException(nameof(deliveryTime), "Delivery in the past");

        while (dt >= emits.Count)
        {
            emits.Add([]);
        }
        emits[dt].Add(axon);
    }
    public void RegisterPotentialExcitation(Neuron neuron)
    {
        potentiallyExcitedDuringStep.Add(neuron);
    }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public IReadOnlyClock Clock => this.Network.Clock;
    public float[] Output => this.Network.Output;
}
