using Serilog.Events;
using Serilog.Formatting;
using bagit.net.domain;
using Microsoft.Extensions.Logging;
using Spectre.Console;

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

public static class Logging
{
    public static void LogEvent(MessageRecord messageRecord, ILogger logger)
    {
        switch(messageRecord.GetLevel()) {
            case MessageLevel.INFO: 
            {
                logger.LogInformation(messageRecord.GetMessage());
                break;
            }
            case MessageLevel.ERROR:
            {
                logger.LogError(messageRecord.GetMessage());
                break;
             }
            case MessageLevel.WARNING:
            {
                logger.LogWarning(messageRecord.GetMessage());
                break;
            }
            default: throw new InvalidDataException("Unknown message level");
        }
    }

    public static void LogEvents(IEnumerable<MessageRecord> records, bool quiet, ILogger logger) {

        foreach (var messageRecord in records)
        {
            switch (messageRecord.GetLevel())
            {
                case MessageLevel.INFO:
                    {
                        if (!quiet)
                            logger.LogInformation(messageRecord.GetMessage());
                        break;
                    }
                case MessageLevel.ERROR:
                    {
                        logger.LogError(messageRecord.GetMessage());
                        break;
                    }
                case MessageLevel.WARNING:
                    {
                        logger.LogWarning(messageRecord.GetMessage());
                        break;
                    }
                default: throw new InvalidDataException("Unknown message level");
            }
        }
    }
} 


