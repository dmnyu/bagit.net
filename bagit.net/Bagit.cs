using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace bagit.net
{
    public static class Bagit
    {
        public const string VERSION = "0.2.1-alpha"; 
        public const string BAGIT_VERSION = "1.0";

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

    }

    public static class ServiceConfigurator
    {
        public static ServiceProvider BuildServiceProvider<TWorker>(string logFile = "")
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
                loggerConfig = loggerConfig.WriteTo.File(logFile);
            }

            Log.Logger = loggerConfig.CreateLogger();

            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddSerilog(Log.Logger, dispose: true));
            services.AddSingleton<IManifestService, ManifestService>();
            services.AddSingleton<IBagInfoService, BagInfoService>();
            services.AddTransient<TWorker>();
            return services.BuildServiceProvider();
        }
    }
}
