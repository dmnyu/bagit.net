using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace bagit.net.cli.Commands;

class BagCommand : Command<BagCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Path to the directory to bag.")]
        [CommandArgument(0, "[path]")]
        public string? Path { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var bagPath = Path.GetFullPath(settings.Path!);
        AnsiConsole.MarkupLine($"[yellow]Creating bag for directory[/] {bagPath}");
        var bagit = new Bagit();
        bagit.CreateBag(bagPath);
        return 0;
    }
}