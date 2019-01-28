## Overview

In a [previous episode](../dotnet_task_memory_usage/), we made a toy C# coroutine.
Then, a kind stranger provided a go version.
So we will compare the CPU and memory usage of a toy C# coroutine (e.g. async method / Task) vs a golang goroutine.


## Methodology

* Close web browsers, etc.

* Run `swapoff -a` so swap isn't hiding memory usage.

* See [run-csharp.sh](run-csharp.sh) and [run-go.sh](run-go.sh)


## Results 

(See blog post)


## Disclaimer

This is a micro-benchmark.  Your application is not a micro-benchmark.

Benchmarks can be gamed.  

Be mindful of [Goodhart's law](https://en.wikipedia.org/wiki/Goodhart%27s_law)

And watch https://www.youtube.com/watch?v=vm1GJMp0QN4&t=17m49s for a hilarious benchmark failure
