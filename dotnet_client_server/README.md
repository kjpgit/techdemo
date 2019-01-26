## Overview

In our [last episode](../dotnet_task_memory_usage/), we measured the memory
overhead of the simplest possible .NET Core task / coroutine.
Now let's step it up and make a network server that echoes back bytes.

Source code for the client and server is [here](Program.cs).
It uses the standard .NET Core networking library, nothing fancy.


## Details

* .NET Core 2.2.102 on Ubuntu 18.04 (`dotnet --version`).  Memory information from `top`.

* Default threadpool settings, which appear to use 10-12 actual threads for the client, on my dual core Pentium

* Unix stream sockets to make it simple (TCP is just annoying, even on loopback)

* Server baseline (1 client): 79 MB RSS / 3260 MB VSS

* Server loaded (500000 clients): 1000-1300 MB RSS / 4430 MB VSS (RSS fluctuates due to GC)

* Delta: 1000-1200 MB RSS, 1200 MB VSS

* Per client overhead (C# server process only, not including kernel): **2000-2400 bytes RSS, 2400 bytes VSS**

* Also, the client process has a very similar memory usage profile as the server process.  They both end up settling at 1GB after a period of time.


## Conclusion

Again, very impressive!  
This is only about 2x of what the toy coroutine allocated.  
It's likely that the memory overhead of the language / runtime is
going to be small compared to your application-specific data.
