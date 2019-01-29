using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace dotnet_massive_async
{
    class Program
    {
        static async Task Main(string[] args)
        {
            RuntimeHelper.PrintConfiguration();

            int numTasks = int.Parse(args[0]);
            TimeSpan sleepDelay = TimeSpan.FromSeconds(double.Parse(args[1]));
            long maxHits = long.Parse(args[2]);

            // NB: This sets the "start time" of the first scoreboard message.  
            var scoreboard = new Scoreboard();

            // Warning: This fires off a ton of tasks and they will start doing work 
            // while we are still constructing other tasks.  This is non-deterministic
            // and in theory produces "thundering herds".  Isn't benchmarking
            // parallel code fun?
            //
            // However, we are not as concerned with wall-clock time as we are about
            // measuring total memory usage and CPU time.
            //
            // But you probably don't want to run this test with a sleepDelay
            // too low and completely saturate your CPU cores, and thus starve
            // this creation loop.  You should verify that the first scoreboard message
            // shows a reasonable "elapsed" time.
            //
            for (var i = 0; i < numTasks; i++) {
                // Note: We used to add this to a list, but that is not necessary.
                // Note: We assign to a variable just to supress a compiler warning
                // that we aren't using await here. 
                var task = WorkTask(scoreboard, sleepDelay);
            }

            await scoreboard.PollScores(maxHits);
        }

        static async Task WorkTask(Scoreboard scoreboard, TimeSpan sleepDelay)
        {
            // NB: The first AddHit() runs synchronously.
            // Only the first await returns control back to the startup loop in Main().
            // 
            // This could be changed to:
            // a) await Task.Yield() at the start of this function
            // b) reorder AddHit() and Task.Delay() (and do the same in the Go code)
            // c) change function signature and use Task.Run()???
            //
            while (true) {
                scoreboard.AddHit();
                await Task.Delay(sleepDelay);
            }
        }
    }


    class RuntimeHelper
    {
        public static void PrintConfiguration()
        {
            // NB: You can override these e.g.:
            // export COMPlus_ThreadPool_ForceMinWorkerThreads=3
            // export COMPlus_ThreadPool_ForceMaxWorkerThreads=3
            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxIOThreads);
            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minIOThreads);
            Console.WriteLine($"Num Processors: {Environment.ProcessorCount}");
            Console.WriteLine($"MinThreads: {minWorkerThreads}, IO={minIOThreads}");
            Console.WriteLine($"MaxThreads: {maxWorkerThreads}, IO={maxIOThreads}");
        }

        // Show current memory usage.  Useful for showing what the GC is doing.
        // /usr/bin/time -v just shows the max usage, which doesn't show the whole
        // picture for a generational GC.
        public static void PrintMemoryInfo() 
        {
            var lines = File.ReadAllText("/proc/self/status").Split('\n');
            foreach (var line in lines) {
                if (line.StartsWith("VmSize:") || line.StartsWith("VmRSS:")) {
                    Console.WriteLine(" - " + line);
                }
            }
        }
    }


    class Scoreboard
    {
        public Scoreboard() { }

        public void AddHit() 
        {
            // I can do 100 M/sec on a Pentium, so this shouldn't be a bottleneck
            Interlocked.Increment(ref hitCount);
        }

        public async Task PollScores(long maxHits) 
        {
            while (true) {
                DateTime currentTime = DateTime.UtcNow;
                TimeSpan elapsed = currentTime - lastDumpTime;
                long totalHits = Interlocked.Read(ref hitCount);

                Console.WriteLine($"elapsed={elapsed} totalHits={totalHits}");
                RuntimeHelper.PrintMemoryInfo();
                lastDumpTime = currentTime;

                if (totalHits >= maxHits) {
                    Environment.Exit(0);
                }

                // Sleep for 1/10 of a second to keep precision reasonable
                await Task.Delay(TimeSpan.FromSeconds(0.1));
            }
        }

        DateTime lastDumpTime = DateTime.UtcNow;
        long hitCount = 0;
    }
}
