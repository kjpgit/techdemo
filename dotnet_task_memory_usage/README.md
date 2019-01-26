## Summary

Measure the memory usage of a barebones C# coroutine (e.g. async/await task).

The task just sleeps and hits a scoreboard in a loop.  See the trivial [source code](Program.cs).


## Details

* .NET Core 2.2.102 on Ubuntu 18.04 (`dotnet --version`).  Memory information from `/proc/self/statm`.

* Default threadpool settings, which appear to use 3-5 actual threads for tasks, on my dual core Pentium

* For 1 worker task, the process uses 8.3MB RSS / 720MB VSS (`dotnet run -c Release -- 1 1`)

* For 1 million worker tasks, the process uses ~200MB RSS / 960MB VSS (`dotnet run -c Release -- 1000000 1`)

* Thus the delta is 192MB RSS / 240MB VSS

* Thus the per-task overhead at 1 million tasks is **192 bytes RSS / 240 bytes VSS**


## Conclusion

Impressive!  Obviously I wouldn't even try 1 million threads ...

I didn't see any real difference between Debug and Release builds.
(It's unfortunate that `dotnet run` defaults to Debug, btw.  Shouldn't that be "dotnet debug"?)

On the 1 million test, there are sometimes some small memory spikes when the GC
can't keep up, but overall it's pretty consistent.


## Coming Next

In our next episode, we will investigate actual socket wakeups (e.g. epoll) to
see scalability limits for an actual client/server.
