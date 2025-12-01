using bagit.net.cli.lib;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;


namespace bagit.net.cli.Commands;

class BagCommand : Command<BagCommand.Settings>
{
    public class Settings : CommandSettings
    {

        [CommandOption("--algorithm")]
        public string? Algorithm { get; set; }

        [CommandOption("--log")]
        public string? logFile { get; set; }

        [CommandArgument(0, "[directory]")]
        [Description("Path to the directory to bag.")]
        public string? Directory { get; set; }

    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var serviceProvider = ServiceConfigurator.BuildServiceProvider<Bagger>(settings.logFile);
            var bagger = serviceProvider.GetRequiredService<Bagger>();
            bagger.CreateBag(settings.Directory, settings.Algorithm, settings.logFile, cancellationToken);
        }
        catch (Exception ex) {
            AnsiConsole.MarkupLine($"[red][bold]ERROR:[/] {ex.Message}");
            return 1;
        }
        return 0;
    }
}