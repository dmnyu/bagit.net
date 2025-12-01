using Serilog.Events;
using Serilog.Formatting;

namespace bagit.net.cli.lib;

public class ShortLevelFormatter : ITextFormatter
{
    private readonly Dictionary<LogEventLevel, string> LevelMap = new()
    {
        [LogEventLevel.Verbose] = "trace",
        [LogEventLevel.Debug] = "debug",
        [LogEventLevel.Information] = "info",
        [LogEventLevel.Warning] = "warn",
        [LogEventLevel.Error] = "error",
        [LogEventLevel.Fatal] = "fatal"
    };

    public void Format(LogEvent logEvent, TextWriter output)
    {
        var timestamp = logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        var level = LevelMap[logEvent.Level];
        var message = logEvent.RenderMessage();

        output.WriteLine($"{timestamp} [{level}] {message}");

        if (logEvent.Exception != null)
        {
            output.WriteLine(logEvent.Exception);
        }
    }
}


