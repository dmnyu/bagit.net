using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace bagit.net.cli.Commands
{
    class ValidateCommand : Command<ValidateCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("--log")]
            public string? logFile { get; set; }

            [CommandOption("--fast")]
            public bool Fast { get; set; }

            [CommandArgument(0, "[directory]")]
            [Description("Path to the bag directory to validate.")]
            public string? Directory { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(settings.Directory))
            {
                AnsiConsole.MarkupLine("[red][bold]ERROR:[/][/]");
                AnsiConsole.MarkupLine("[red]a directory to a BagIt bag must be specified when creating a bag[/]\n");
                BagitCLI.app.Run(new string[] { "help" }, cancellationToken);
                return 1;
            }

            var bagPath = Path.GetFullPath(settings.Directory);
            if (!Directory.Exists(bagPath))
            {
                AnsiConsole.MarkupLine("[red][bold]ERROR:[/][/]");
                AnsiConsole.MarkupLine($"[red]the directory {bagPath} does not exist[/]\n");
                BagitCLI.app.Run(new string[] { "help" }, cancellationToken);
                return 1;
            }

            if (!string.IsNullOrWhiteSpace(settings.logFile))
            {
                AnsiConsole.MarkupLine($"bagit.net.cli v{Bagit.VERSION}");
                AnsiConsole.MarkupLine($"Logging to {settings.logFile}");
            }
            var services = new ServiceCollection();

            // Logging (Serilog bridge)
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(new ShortLevelFormatter()) // your custom formatter
                .CreateLogger();
            
            services.AddLogging(builder => builder.AddSerilog(Log.Logger, dispose: true));

            // Your services
            services.AddSingleton<IManifestService, ManifestService>();
            services.AddSingleton<IBagInfoService, BagInfoService>();
            services.AddTransient<Validator>();
            var serviceProvider = services.BuildServiceProvider();

            var validator = serviceProvider.GetRequiredService<Validator>();
            validator.ValidateBag(bagPath, settings.Fast);

            return 0;
        }
    }
}


