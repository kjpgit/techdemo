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
            lock (this) {
                long current = threadScore.GetValueOrDefault(threadId, 0);
                threadScore[threadId] = current + 1;
            }
        }

        // Run forever.  Print stats every second.
        public async Task PollScores() {
            while (true) {
                await Task.Delay(TimeSpan.FromSeconds(1.0));
                lock (this) {
                    int numThreads = threadScore.Keys.Count;
                    long totalHits = threadScore.Values.Sum();
                    DateTime currentTime = DateTime.UtcNow;
                    TimeSpan elapsed = currentTime - lastDumpTime;
                    Console.WriteLine($"elapsed={elapsed} numThreads={numThreads}" +
                        $" totalHits={totalHits}");
                    Console.WriteLine(GetMemInfo());
                    threadScore.Clear();
                    lastDumpTime = currentTime;
                }
            }
        }

        private string GetMemInfo() {
            var data = File.ReadAllText("/proc/self/statm").Split(' ');
            return $"(NB: 4K pages) vmsize={data[0]} vmrss={data[1]}";
        }

        DateTime lastDumpTime = DateTime.UtcNow;

        // threadId -> operation count
        Dictionary<int, long> threadScore = new Dictionary<int, long>();
    }
}
