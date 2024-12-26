using JBSnorro.NN;

namespace Tests.JBSnorro.NN;

// OnTickEventArgs presents the internal state of the machine to the outside world
// here we define what events happen "within" a timestep and in which order.
// we have the following events:
// charge decay
// axon delivery
// neuron activation
// register output
//
// I would say that when a machine starts, we start with neuron activation
// that means that if they start with sufficient charge, they fire immediately
// suppose that the length of such an axon is 1, i.e. dt=1, then it makes sense that near the next tick it delivers
// we imagine that we have a tick of the clock, when certain things happen, then a while nothing is happening except for the traveling of charge along axons
// and then around the second tick of the clock things are happening again.
// maybe we can also the decay to happen "in the middle" between two ticks.
// 
// the output should be registered before decay, after axon delivery, and ideally _just_ before the next tick. 
//
// T=0                                                                                                                                     T=1
// tick, neurons fire                                 charge decays, axons propagate                        axons deliver, register output, tock
// The only difference is that at time=-1 we start at axon delivery
//
// tick is easily defined. neurons fire too: UpdateNeurons(). Axon delivery is also trivial: DeliverFiredAxons()
// that leaves charge decay, axon propagation, output registration.
// charge decay happens at the end in UpdateNeurons(). So that's good. Just questioning whether charge decay should happen _just after_ neuron firing? well, in code yes, but metaphorically
// there's time in between, as though it's happening slowly.
// axons propagate and axons deliver are next to each other. That's good, because axon propagation's effect is mainly seen at axon delivery.
// So we can essentially pretend it's one step. axon propagation is tacit, by virtue of time ticking.
// output registration though: it makes sense that it's at a tick right. And just before neurons fire, yes.



// currently it's:
// tick, axons deliver, register output, process feedback, neurons fire, neurons decay, onTicked.invoke, tock
// I also like it if onTicked.invoke and process feedback are "simultaneous", because then only at one point in the cycle can the user interact/introspect.
// I also like it if it were just before or after a tick.
// so that means the code should be:
// UpdateNeurons()
// DeliverAxons()
//
// output = network.Output;
// ProcessFeedback()
// onTicked?.Invoke()

// -------------
// damnit. The above doesn't work because neurons fire and the axons deliver within a time step but the axons would have to deliver that of the next time step already!
// T=0                                                                                                                                     T=1
// axons deliver, register output,                neurons fire                                 charge decays, axons propagate                        

// public class OnTickEventArgsTests
// {
//     [Fact]
//     public void A_network()
//     {
//         OnTickEventArgs e;

//         e.
//     }
// }
