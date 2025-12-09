using bagit.net.interfaces;
using bagit.net.services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace bagit.net.tests
{

    public static class ChecksumServiceConfigurator
    {
        public static ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            var logger = DefaultLogger.GetDefaultLogger();
            services.AddLogging(builder => builder.AddSerilog(logger, dispose: true));
            services.AddSingleton<IChecksumService, ChecksumService>();
            services.AddSingleton<IMessageService, MessageService>();
            return services.BuildServiceProvider();
        }
    }

    public static class ManifestServiceConfigurator
    {
        public static ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            var logger = DefaultLogger.GetDefaultLogger();
            services.AddLogging(builder => builder.AddSerilog(logger, dispose: true));
            services.AddSingleton<IManifestService, ManifestService>();
            services.AddSingleton<IChecksumService, ChecksumService>();
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IFileManagerService, FileManagerService>();
            return services.BuildServiceProvider();
        }
    }

    public static class TagFileServiceConfigurator
    {
        public static ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            var logger = DefaultLogger.GetDefaultLogger();
            services.AddLogging(builder => builder.AddSerilog(logger, dispose: true));
            services.AddSingleton<ITagFileService, TagFileService>();
            services.AddSingleton<IFileManagerService, FileManagerService>();
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IManifestService, ManifestService>();
            services.AddSingleton<IChecksumService, ChecksumService>();
            return services.BuildServiceProvider();
        }
    }

    public static class FileManagerServiceConfigurator
    {
        public static ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            var logger = DefaultLogger.GetDefaultLogger();
            services.AddLogging(builder => builder.AddSerilog(logger, dispose: true));
            services.AddSingleton<IFileManagerService, FileManagerService>();
            services.AddSingleton<IMessageService, MessageService>();
            return services.BuildServiceProvider();
        }
    }

    public static class ValidationServiceConfigurator
    {
        public static ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            var logger = DefaultLogger.GetDefaultLogger();
            services.AddLogging(builder => builder.AddSerilog(logger, dispose: true));
            services.AddSingleton<IValidationService, ValidationService>();
            services.AddSingleton<ITagFileService, TagFileService>();
            services.AddSingleton<IFileManagerService, FileManagerService>();
            services.AddSingleton<IManifestService, ManifestService>();
            services.AddSingleton<IChecksumService, ChecksumService>();
            services.AddSingleton<IMessageService, MessageService>();
            return services.BuildServiceProvider();
        }
    }

    public static class DefaultServiceConfigurator
    {
        public static ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            var logger = DefaultLogger.GetDefaultLogger();
            services.AddLogging(builder => builder.AddSerilog(logger, dispose: true));
            services.AddSingleton<IChecksumService, ChecksumService>();
            services.AddSingleton<ICreationService, CreationService>();
            services.AddSingleton<IFileManagerService, FileManagerService>();
            services.AddSingleton<IManifestService, ManifestService>();
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<ITagFileService, TagFileService>();
            services.AddSingleton<IValidationService, ValidationService>();
            return services.BuildServiceProvider();
        }
    }

    static class DefaultLogger
    {
        public static Logger GetDefaultLogger()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(new ShortLevelFormatter())
                .CreateLogger();
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
            var message = logEvent.RenderMessage();

            output.WriteLine($"{timestamp} [{level}] {message}");

            if (logEvent.Exception != null)
            {
                output.WriteLine(logEvent.Exception);
            }
        }
    }


}
