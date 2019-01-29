## Overview

In a [previous episode](../dotnet_task_memory_usage/), we made a toy C# coroutine.
Then, a kind stranger provided a go version.
So we will compare the CPU and memory usage of a toy C# coroutine (e.g. async method / Task) vs a golang goroutine.


## Methodology

* Close web browsers, etc.

* Run `swapoff -a` so swap isn't hiding memory usage.

* See [run-csharp.sh](run-csharp.sh) and [run-go.sh](run-go.sh)


## Results 

* Jan 2019: [medium post](https://medium.com/@karl.pickett/benchmarking-a-toy-c-task-vs-a-go-goroutine-is-there-any-difference-248f73f7f7b7)


## Disclaimer

* This is a micro-benchmark.  Your application is not a micro-benchmark.

* Benchmarks can be gamed. Benchmarks are hard to do correctly.

* Be mindful of Goodhart’s law: “When a measure becomes a target, it ceases to be a good measure.”

* This code is not designed for high core count machines due to the single (not sharded) atomic counter

* And watch https://www.youtube.com/watch?v=vm1GJMp0QN4&t=17m49s for a hilarious benchmark failure
