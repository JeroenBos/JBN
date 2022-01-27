namespace JBSnorro.NN;

sealed class Machine
{
    private readonly Network network;
    private int maxTime = -1;

    public Machine(Network network)
    {
        this.network = network;
        this.potentiallyActivatedDuringStep = new List<Neuron>();
        this.emits = new List<List<Axon>>();
        this.emits.Add(new List<Axon>());
    }

    public float[,] Run(int maxTime)
    {
        if (this.t != -1) throw new InvalidOperationException("This machine has already run");
        if (this.emits[0].Count != 0) throw new Exception("this.emits[0].Count == 0");
        this.maxTime = maxTime;
        this.emits.RemoveAt(0);  // if t starts at -1, input neurons with length 1 add to this.emits[1]
        this.emits.Add(new List<Axon>()); // so we need to skip this.emits[0].

        var output = new float[maxTime, this.network.output.Length];
        // assumes the input axioms have been triggered
        for (this.t = 0; t < maxTime; t++)
        {
            this.Tick();

            for (int i = 0; i < this.network.output.Length; i++)
            {
                output[t, i] = this.network.output[i];
            }
            this.network.Decay(this.t + 1);
        }
        return output;
    }
    private void Tick()
    {
        var emittingAxons = this.emits[0];
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

        Console.WriteLine($"t={this.t:d2}, emits: {emittingAxons.Count}(Σ={positiveCumulativeOomph:n2}/-{negativeCumulativeOomph:n2}), acts: {activationCount}");

        // clean up
        emittingAxons.Clear();
        this.emits.RemoveAt(0);
        this.emits.Add(emittingAxons);
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

    private readonly List<Neuron> potentiallyActivatedDuringStep;
    private readonly List<List<Axon>> emits;
    private int t = -1;
    internal int Time => t;
    public void AddEmitAction(int time, Axon axon)
    {
        if (time >= this.maxTime && this.maxTime != -1)
            return;

        int dt = time - t;
        if (dt <= 0) throw new Exception("dt <= 0");
        while (dt >= emits.Count)
        {
            emits.Add(new List<Axon>());
        }
        emits[dt].Add(axon);
    }
    public void RegisterPotentialActivation(Neuron neuron)
    {
        this.potentiallyActivatedDuringStep.Add(neuron);
    }
}
