using Xunit;
using JBSnorro.NN;

public class NetworkTests
{
    [Fact]
    public void CreateNetwork()
    {
        var connections = new AxonType[1, 1] { { AxonTypes.One } };
        var network = new Network(NeuronTypes.OnlyOne,
                                  inputCount: 1,
                                  outputCount: 1,
                                  connections,
                                  GetLengthFunctions.One,
                                  GetInitialWeightFunctions.One);
    }

    [Fact]
    public void RunUnactivatedNetwork()
    {
        var connections = new AxonType[1, 1] { { AxonTypes.One } };
        var network = new Network(NeuronTypes.OnlyA,
                                  inputCount: 1,
                                  outputCount: 1,
                                  connections,
                                  GetLengthFunctions.One,
                                  GetInitialWeightFunctions.One);

        var machine = new Machine(network);
        var output = machine.Run(1);
        Assert.Equal(output, new float[,] { { 0 } });
    }

    
    [Fact]
    public void RunActivatedNetworkOfOne()
    {
        var connections = new AxonType?[1, 1] { { null }}; 
        var network = new Network(NeuronTypes.OnlyA,
                                  inputCount: 1,
                                  outputCount: 1,
                                  connections,
                                  GetLengthFunctions.One,
                                  GetInitialWeightFunctions.One);


        var machine = new Machine(network);
        network.Input[0].Activate(machine);
        
        var output = machine.Run(1);
        Assert.Equal(output, new float[,] { { 1 } });
    }
    
}
