using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Serilog;
using Serilog.Events;
using Serilog.Formatting;

namespace bagit.net
{
    public static class Bagit
    {
        public const string VERSION = "0.1.0";
        public const string BAGIT_VERSION = "1.0";

        public static ILogger Logger { get; private set; } = null!;

        public static void InitLogger(string? logFileLocation = null)
        {
            ITextFormatter formatter = new ShortLevelFormatter();

            var loggerConfig = new LoggerConfiguration().MinimumLevel.Debug();

            if (!string.IsNullOrWhiteSpace(logFileLocation))
            {
                if (File.Exists(logFileLocation)) {
                    File.Delete(logFileLocation);
                }
                loggerConfig = loggerConfig.WriteTo.File(
                    formatter,
                    logFileLocation
                );
            } else {
                loggerConfig.WriteTo.Console(formatter);
            }

                Log.Logger = loggerConfig.CreateLogger();

            // Bridge Serilog → ILogger
            var factory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(Log.Logger, dispose: true);
            });

            Logger = factory.CreateLogger(typeof(Bagit).FullName ?? "bagit.net");
        }

        public static Dictionary<string, ChecksumAlgorithm> Algorithms = new Dictionary<string, ChecksumAlgorithm>()
        {
            {"md5", ChecksumAlgorithm.MD5},
            {"sha1", ChecksumAlgorithm.SHA1},
            {"sha256", ChecksumAlgorithm.SHA256},
            {"sha384", ChecksumAlgorithm.SHA384},
            {"sha512", ChecksumAlgorithm.SHA512}
        };

        public const string checksumPattern = @"-(md5|sha1|sha256|sha384|sha512)\b";
        public const string ManifestPattern = @"manifest-(md5|sha1|sha256|sha384|sha512).txt";
        public const string TagmanifestPattern = @"tagmanifest-(md5|sha1|sha256|sha384|sha512).txt";

        public class ShortLevelFormatter : ITextFormatter
        {
            private static readonly Dictionary<LogEventLevel, string> LevelMap = new()
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
}
