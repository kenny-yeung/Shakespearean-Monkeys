# Shakespearean-Monkeys
University project for a distributed application that illustrates the 
Shakespearean Monkeys genetic algorithm using standalone REST carter servers to send POST and empty OK responses.
I only had to create the Monkeys file, everything else was given.

If you run the Monkeys batch, then Fitness batch, and finally the client tests, you can see the two carter servers intereacting with each other.
The target text is sent to the fitness server, which is then posted to the monkeys through POST /try, and then the genome of the target text is sent back to the client

