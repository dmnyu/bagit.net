using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bagit.net.cli.Commands
{
    public static class ServiceConfigurator
    {
        public static ServiceProvider BuildServiceProvider<TWorker>()
            where TWorker : class
        {
            var services = new ServiceCollection();

            // Logging (Serilog bridge)
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(new ShortLevelFormatter())
                .CreateLogger();

            services.AddLogging(builder => builder.AddSerilog(Log.Logger, dispose: true));

            // Shared services
            services.AddSingleton<IManifestService, ManifestService>();
            services.AddSingleton<IBagInfoService, BagInfoService>();

            // Command-specific worker type
            services.AddTransient<TWorker>();

            return services.BuildServiceProvider();
        }
    }
}
