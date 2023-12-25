In the DNA side, I need to map to:
Machine.Feedback           output -> (float, float)     (fixed per puzzle)
Network.InputCount         int                          (fixed per puzzle)
Network.OutputCount        int                          (fixed per puzzle)
Network.NeuronTypes:       enum[]                       (hint: A1)
Network.AxonTypes:         enum[]                       (hint: A1)
AxonType.getLength         (int, int) -> int            (trivial computation)
AxonType.initialWeight:    (int, int) -> float          (hint: distribution sampling. performance not important)
AxonType.getUpdatedWeight: (float, int, float, int, float, float) -> float  (some form of correlations?)
AxonType.Count             int
NeuronType.GetDecay:       (int, int) -> float          (hint: 2x independent F1)
NeuronType.Count           int


What is the type of function again that I had envisioned?
Let's assume to build that function I get a set of numbers N that change slowly when the underlying DNA changes,
then any combination of N into a function will also be a slow change. But I suppose some are better than others.
F1:
  I had imagined that simply each number represents the height of the graph in predefined steps
F2:
  I'm sure you could make a taylor series out of it. Maybe that's more suited for resilience.
  Or Fourier series for that matter. I think I like fourier series better.
  s(x) = a + Σ_n^N (a_n cos(n x) + b_n sin(n y))

A1:
  I had imagined any element of an enum array can be represented by the bits 01 with a bytes backing field
  The mutation of the 0 increments the byte backing field, mutation of the 1 decrements.
  Given that has no direct paths between nonadjacent paths, it may be a better idea to have it represented
  as a string of bits, each representing a different enum value, the mutation of which alters to that enum.
  The actual type can surely be encoded cleverly in that string of bits even, if so desired.


# Open questions
How to make the NN train on shorter equivalent result is better?
How to mutate the length of an array?
I definitely need junk DNA that can be transformed into live DNA. How?
It would be lovely if it could define self-mutation rates. Possible different rates per section of DNA.
Then, combined with incorporating the computational cost into fitness, efficiency could be built into the algorithm.

updatedWeight params: weight, timeSinceLastActivation, averageTimeBetweenActivations, activationCount, dopamine, cortisol.
estimated ranges:   [-10, 10]f,    [0, 100]i,                  [0, 50]f,                 [0, 20]i,     [0, 1]f,   [0, 1]f

Multidimensional fourier? 

a + Π_d^D Σ_n^N (a_nd cos(n x) + b_nd sin(n y))
number of parameters = 1 + D X N 
