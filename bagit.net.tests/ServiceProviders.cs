using bagit.net.interfaces;
using bagit.net.services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

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
    

}
