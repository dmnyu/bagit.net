using Serilog.Events;
using Serilog.Formatting;
using Spectre.Console;

namespace bagit.net.domain;
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

        // Escape both the level prefix and the message for Spectre
        var safeLevel = Markup.Escape(level);
        var safeMessage = Markup.Escape(logEvent.RenderMessage());

        output.WriteLine($"{timestamp} [{safeLevel}] {safeMessage}");

        if (logEvent.Exception != null)
        {
            // Write exceptions safely as plain text
            var exceptionText = Markup.Escape(logEvent.Exception.ToString());
            output.WriteLine(exceptionText);
        }
    }
}