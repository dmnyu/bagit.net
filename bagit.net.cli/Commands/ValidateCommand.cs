// Validation command
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading;

namespace bagit.net.cli.Commands;

class ValidateCommand : Command<ValidateCommand.ValidateSettings>
{
    public class ValidateSettings : CommandSettings
    {
        [Description("Path to the directory to bag.")]
        [CommandArgument(0, "[path]")]
        public string? Path { get; set; }
    }

    public override int Execute(CommandContext context, ValidateSettings settings, CancellationToken cancellationToken)
    {
        var bagPath = Path.GetFullPath(settings.Path!);
        AnsiConsole.MarkupLine($"[cyan]Validating bag at:[/] {bagPath}");
        // TODO: Add validation logic here
        Console.WriteLine("Validation not implemented yet");
        return 0;
    }
}