## Summary

Measure the memory usage of a barebones C# coroutine (e.g. async method / task).

Each task just sleeps and hits a scoreboard in a loop.  See the trivial [source code](Program.cs).


## Details

* .NET Core 2.2.102 on Ubuntu 18.04 (`dotnet --version`).  Memory information from `/proc/self/statm`.

* Default threadpool settings, which appear to use 3-5 actual threads for tasks, on my dual core Pentium

* For 1 worker task, the process uses 34MB RSS / 2800MB VSS (`dotnet run -c Release -- 1 1`)

* For 1 million worker tasks, the process uses ~850B RSS / 3950MB VSS (`dotnet run -c Release -- 1000000 1`)

* Thus the delta is 816MB RSS / 1150MB VSS

* Thus the per-task overhead at 1 million tasks is **816 bytes RSS / 1150 bytes VSS**


## Conclusion

Impressive!  Obviously I wouldn't even try 1 million threads ...

I didn't see any real difference between Debug and Release builds.
(It's unfortunate that `dotnet run` defaults to Debug, btw.  Shouldn't that be "dotnet debug"?)

On the 1 million test, there are sometimes some small memory spikes /
fluctuations, but overall it's pretty consistent.  
The CPU isn't maxed out, so not sure why the GC isn't keeping up.


## Coming Next

In our next episode, we will investigate actual socket wakeups (e.g. epoll) to
see scalability limits for an actual client/server.
