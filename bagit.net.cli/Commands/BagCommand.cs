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
        public string? LogFile { get; set; }

        [CommandOption("--quiet")]
        public bool Quiet { get; set; }

        [CommandOption("--tag-file")]
        public string? TagFile {  get; set; }

        [CommandArgument(0, "[directory]")]
        [Description("Path to the directory to bag.")]
        required public string Directory { get; set; }

    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var serviceProvider = ServiceConfigurator.BuildServiceProvider<Bagger>(settings.LogFile);
            var bagger = serviceProvider.GetRequiredService<Bagger>();
            bagger.CreateBag(settings.Directory, settings.Algorithm, settings.TagFile, settings.Quiet, settings.LogFile, cancellationToken);
        }
        catch (Exception ex) {
            AnsiConsole.MarkupLine($"[red][bold]ERROR:[/] {ex.Message}");
            return 1;
        }
        return 0;
    }
}