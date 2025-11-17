using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace bagit.net
{
    public static class Bagit
    {
        public const string VERSION = "0.1.0";
        public const string BAGIT_VERSION = "1.0";


        public static ILogger Logger { get; set; } = null!;
        public static void InitLogger()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddSimpleConsole(options =>
                    {
                        options.SingleLine = true;
                        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                    })
                    .SetMinimumLevel(LogLevel.Debug);
            });

            Logger = loggerFactory.CreateLogger("");
        }

    }
}
