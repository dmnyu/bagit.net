using bagit.net.interfaces;
using bagit.net.services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace bagit.net.domain
{
    public static class BagitServiceProvider
    {
        public static ServiceProvider BuildServiceProvider<TWorker>(string? logFile = "")
            where TWorker : class
        {
            var loggerConfig = new LoggerConfiguration().MinimumLevel.Debug();

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

    public static class BagItServiceCollectionExtensions
    {
        public static IServiceCollection AddBagIt(this IServiceCollection services)
        {
            services.AddLogging();
            services.AddSingleton<CreationService>();
            services.AddSingleton<ValidationService>();
            // etc

            return services;
        }
    }
}
