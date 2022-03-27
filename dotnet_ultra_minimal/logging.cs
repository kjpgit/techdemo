// Super simple logging that actually works
// (If you are in a container that just needs to get stdout/stderr)
//
// Unlike the built in console logger that delays output:
// https://stackoverflow.com/questions/50221983/cant-get-console-logging-to-work-in-net-core-2-0
//

public sealed class SimpleLogger  : ILogger
{
    private readonly string _name;
    private readonly LogLevel _min_level;

    public SimpleLogger (
        string name,
        LogLevel min_level
        )
    {
        _name = name;
        _min_level = min_level;
    }

    public IDisposable BeginScope<TState>(TState state) => default!;

    public bool IsEnabled(LogLevel logLevel) {
        return logLevel >= _min_level;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (logLevel >= LogLevel.Warning) {
            Console.WriteLine($"[{logLevel}] {_name}: {formatter(state, exception)}");
        } else {
            Console.WriteLine($"{_name}: {formatter(state, exception)}");
        }
    }
}
