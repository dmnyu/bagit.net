using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace bagit.net.cli.Commands;

class BagCommand : Command<BagCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Path to the directory to bag (optional).")]
        [CommandArgument(0, "[path]")]
        public string? Path { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var path = settings.Path ?? Directory.GetCurrentDirectory();
        AnsiConsole.MarkupLine($"[yellow]Creating bag at:[/] {path}");
        // TODO: Add bagging logic here
        return 0;
    }
}