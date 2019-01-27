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

* Server loaded (500000 persistent client connections, sending requests every 30 seconds): 1000-1300 MB RSS / 4430 MB VSS (RSS fluctuates due to GC) (`dotnet run -c Release -- client 500000 30`)

* Delta: 1000-1200 MB RSS, 1200 MB VSS

* Per client overhead (C# server process only, not including kernel): **2000-2400 bytes RSS, 2400 bytes VSS**

* Also, the client process has a very similar memory usage profile as the server process.  They both end up settling at 1GB RSS after a period of time.


## Conclusion

Again, very impressive!
This is only about 2x of what the toy coroutine allocated.
It's likely that the memory overhead of the language / runtime is
going to be small compared to your application-specific data.

Obviously C/C++/Rust (or unsafe C#) with handwritten state machines could make this way smaller, e.g. under 100 bytes. 
But at some point there are diminishing returns, and also developer
productivity, bugs, and security are big tradeoffs.

I appreciate the exceptions thrown by the runtime when I was up against max file descriptor limits.
"Errors must not pass silently" - I'm glad C# agrees.

I also appreciate that everything "just worked" with 500K Unix sockets.
I was expecting something in the runtime (e.g. epoll) to break.  .NET just keeps surprising me.


## Additional Testing Tips

- When testing, note that `/proc/self/statm` is in *4K pages*, unlike `top`.  

- Run `swapoff -a` so swap isn't hiding memory usage.

- To increase file descriptors in a GUI session: Set in `DefaultLimitNOFILE=2000000` in `/etc/systemd/system.conf` and `/etc/systemd/user.conf`.  Then reboot (yes, reboot).  Also update `/proc/sys/fs/file-max` to 3000000.  Alternatively, just spawn some shells via root...
