using bagit.net.interfaces;
using bagit.net.services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Formatting;
using Spectre.Console;

namespace bagit.net.domain
{
    public static class BagitServiceProvider
    {
        public static ServiceProvider BuildServiceProvider<TWorker>(string? logFile = "")
            where TWorker : class
        {
            var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Debug();

            if (string.IsNullOrEmpty(logFile))
            {
                loggerConfig = loggerConfig.WriteTo.Console(new ShortLevelFormatter());
            }
            else
            {
                loggerConfig = loggerConfig.WriteTo.File(new ShortLevelFormatter(), logFile);
            }

            Log.Logger = loggerConfig.CreateLogger();

            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddSerilog(Log.Logger, dispose: true));
            services.AddSingleton<IManifestService, ManifestService>();
            services.AddSingleton<IChecksumService, ChecksumService>();
            services.AddSingleton<ITagFileService, TagFileService>();
            services.AddSingleton<IFileManagerService, FileManagerService>();
            services.AddSingleton<IValidationService, ValidationService>();
            services.AddSingleton<ICreationService, CreationService>();
            services.AddSingleton<IMessageService, MessageService>();
            services.AddTransient<TWorker>();
            return services.BuildServiceProvider();
        }
    }

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
}
