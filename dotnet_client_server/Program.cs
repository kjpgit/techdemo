using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace dotnet_massive_async
{
    class Program
    {
        static string SocketName = "/tmp/csharp_unix.socket";

        static async Task Main(string[] args)
        {
            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxIOThreads);
            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minIOThreads);
            Console.WriteLine($"MinThreads: {minWorkerThreads}, {minIOThreads}");
            Console.WriteLine($"MaxThreads: {maxWorkerThreads}, {maxIOThreads}");

            var scoreboard = new Scoreboard();

            string command = args[0];
            if (command == "client") {
                int numConnections = int.Parse(args[1]);
                double sleepSeconds = double.Parse(args[2]);
                await RunClient(numConnections, sleepSeconds, scoreboard);
            } else if (command == "server") {
                await RunServer();
            } else {
                throw new Exception("unknown command");
            }
        }

        static async Task RunClient(int numConnections, double sleepSeconds,
            Scoreboard scoreboard)
        {
            var localEndPoint = new UnixDomainSocketEndPoint(SocketName);

            for (var i = 0; i < numConnections; i++) {
                Socket sock = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                sock.Connect(localEndPoint);
                var clientTask = StartClient(sock, sleepSeconds, scoreboard);
                Console.WriteLine($"Connected {i}");
            }
            await scoreboard.PollScores();
        }

        static async Task StartClient(Socket sock, double sleepSeconds, Scoreboard scoreboard)
        {
            try {
                using (var stream = new NetworkStream(sock)) {
                    var buffer = new byte[1];
                    var hello = ASCIIEncoding.ASCII.GetBytes("*");
                    while (true) {
                        await stream.WriteAsync(hello, 0, 1);

                        var len = await stream.ReadAsync(buffer, 0, 1);
                        if (len == 0) {
                            // server closed
                            break;
                        }
                        Trace.Assert(len == 1);
                        Trace.Assert(buffer[0] == hello[0]);

                        scoreboard.AddHit();

                        await Task.Delay(TimeSpan.FromSeconds(sleepSeconds));
                    }
                }
            } catch (Exception e) {
                FatalError(e);
            } finally {
                sock.Close();
            }
        }

        static async Task RunServer()
        {
            var localEndPoint = new UnixDomainSocketEndPoint(SocketName);
            Socket listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            listener.Bind(localEndPoint);
            listener.Listen(100);

            while (true) {
                Socket clientSocket = await listener.AcceptAsync();
                Task t = StartServerTask(clientSocket);
            }
        }

        // Read a byte and write it back to the client, in a loop
        static async Task StartServerTask(Socket sock) 
        {
            try {
                using (var stream = new NetworkStream(sock)) {
                    var buffer = new byte[1];
                    while (true) {
                        var len = await stream.ReadAsync(buffer, 0, 1);
                        if (len == 0) {
                            // client closed
                            break;
                        }
                        Trace.Assert(len == 1);
                        await stream.WriteAsync(buffer, 0, 1);
                    }
                }
            } catch (Exception e) {
                FatalError(e);
            } finally {
                sock.Close();
            }
        }

        static void FatalError(Exception e)
        {
            Console.WriteLine($"Fatal error: ${e}");
            Console.WriteLine(e.StackTrace);
        }
    }


    class Scoreboard
    {
        public Scoreboard() { }

        // Record a hit for the current thread 
        // This shouldn't add a noticeable memory load, 
        // since there are so few physical threads.
        public void AddHit() 
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            lock (this) {
                long current = threadScore.GetValueOrDefault(threadId, 0);
                threadScore[threadId] = current + 1;
            }
        }

        // Run forever.  Print stats every second.
        public async Task PollScores() 
        {
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

        // Be careful, statm shows units in pages, not 1K units.
        private string GetMemInfo() 
        {
            var data = File.ReadAllText("/proc/self/statm").Split(' ');
            return $"(NB: 4K Pages) vmsize={data[0]} vmrss={data[1]}";
        }

        DateTime lastDumpTime = DateTime.UtcNow;

        // threadId -> operation count
        Dictionary<int, long> threadScore = new Dictionary<int, long>();
    }
}
