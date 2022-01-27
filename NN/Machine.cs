namespace JBSnorro.NN;

class Machine
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
        for (int t = 0; t < maxTime; t++)
        {
            this.t = t;
            this.Tick();

            for (int i = 0; i < this.network.output.Length; i++)
            {
                output[t, i] = this.network.output[i];
            }
        }
        return output;
    }
    private void Tick()
    {
        var emittingAxons = this.emits[0];
        foreach (Axon axon in emittingAxons)
        {
            axon.Emit(this);
        }
        foreach (Neuron neuron in potentiallyActivatedDuringStep)
        {
            if (neuron.Charge >= Neuron.threshold)
            {
                neuron.Activate(this);
            }
        }

        // clean up
        emittingAxons.Clear();
        this.emits.RemoveAt(0);
        this.emits.Add(emittingAxons);
        potentiallyActivatedDuringStep.Clear();
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
