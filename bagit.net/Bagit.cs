using bagit.net.interfaces;
using bagit.net.services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace bagit.net
{
    public static class Bagit
    {
        public const string VERSION = "0.2.3-alpha"; 
        public const string BAGIT_VERSION = "1.0";

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
            services.AddSingleton<IChecksumService, ChecksumService>();
            services.AddSingleton<ITagFileService, TagFileService>();
            services.AddSingleton<IFileManagerService, FileManagerService>();
            services.AddSingleton<IValidationService, ValidationService>();
            services.AddTransient<TWorker>();
            return services.BuildServiceProvider();
        }
    }
}
