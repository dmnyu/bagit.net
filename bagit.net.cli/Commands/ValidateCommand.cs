// Validation command
using Spectre.Console;
using Spectre.Console.Cli;

namespace bagit.net.cli.Commands;

class ValidateCommand : Command<ValidateCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<path>")]
        public string Path { get; set; } = default!;
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLine($"[cyan]Validating bag at:[/] {settings.Path}");
        // TODO: Add validation logic here
        return 0;
    }
}