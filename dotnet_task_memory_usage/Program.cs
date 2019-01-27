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
            int numTasks = int.Parse(args[0]);
            double sleepSeconds = double.Parse(args[1]);

            //ThreadPool.SetMaxThreads(2, 2);

            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxIOThreads);
            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minIOThreads);
            Console.WriteLine($"MinThreads: {minWorkerThreads}, {minIOThreads}");
            Console.WriteLine($"MaxThreads: {maxWorkerThreads}, {maxIOThreads}");

            var scoreboard = new Scoreboard();

            var taskList = new List<Task>();
            for (var i = 0; i < numTasks; i++) {
                var task = WorkTask(scoreboard, sleepSeconds);
                taskList.Add(task);
            }

            await scoreboard.PollScores();
        }

        static async Task WorkTask(Scoreboard scoreboard, double sleepSeconds)
        {
            while (true) {
                await Task.Delay(TimeSpan.FromSeconds(sleepSeconds));
                scoreboard.AddHit();
            }
        }
    }


    class Scoreboard
    {
        public Scoreboard() { }

        // Record a hit for the current thread 
        // This shouldn't add a noticeable memory load, 
        // since there are so few physical threads.
        public void AddHit() {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Interlocked.Increment(ref this.score);
        }

        // Run forever.  Print stats every second.
        public async Task PollScores() {
            for (int i = 0; i < 10; i++) {
                await Task.Delay(TimeSpan.FromSeconds(1.0));
                long totalHits = Interlocked.Read(ref this.score);
                DateTime currentTime = DateTime.UtcNow;
                TimeSpan elapsed = currentTime - lastDumpTime;
                Console.WriteLine($"elapsed={elapsed} totalHits={totalHits}");
                Console.WriteLine(GetMemInfo());
                Interlocked.Exchange(ref this.score, 0);
                lastDumpTime = currentTime;
            }
        }

        private string GetMemInfo() {
            var data = File.ReadAllText("/proc/self/statm").Split(' ');
            return $"(NB: 4K pages) vmsize={data[0]} vmrss={data[1]}";
        }

        DateTime lastDumpTime = DateTime.UtcNow;

        long score = 0;
    }
}
