using Spectre.Console;
using Spectre.Console.Cli;

namespace bagit.net.cli.Commands;

class HelpCommand : Command<HelpCommand.Settings>
{
    public class Settings : CommandSettings { }
    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        // Header
        AnsiConsole.MarkupLine($"bagit.net.cli v{Bagit.VERSION}");
        AnsiConsole.WriteLine("- a BagIt (RFC 8493) tool for creating and validating bags");
        AnsiConsole.WriteLine();

        // Usage
        AnsiConsole.MarkupLine("[lime]USAGE:[/]");
        AnsiConsole.MarkupLine("  bagit.net.cli.exe [cyan][[COMMAND]][/] [darkorange][[OPTIONS]][/]");
        AnsiConsole.WriteLine();

        // Commands
        AnsiConsole.MarkupLine("[cyan]COMMANDS:[/]");
        AnsiConsole.MarkupLine("  [cyan]create[/]\tcreate a bag from an existing directory");
        AnsiConsole.MarkupLine("  [cyan]help[/]\t\tdisplay the help page");
        AnsiConsole.WriteLine();

        // Create Command
        AnsiConsole.MarkupLine("[cyan]CREATE COMMAND[/]");
        AnsiConsole.MarkupLine("[lime]USAGE:[/]");
        AnsiConsole.MarkupLine("  bagit.net.cli.exe [cyan]create[/] [darkorange][[OPTIONS]][/] [red]<directory>[/]");
        AnsiConsole.WriteLine();

        // Create Options
        AnsiConsole.MarkupLine("[darkorange]OPTIONS[/]");
        AnsiConsole.MarkupLine("  --algorithm\tchecksum algorithm, Values: md5, sha1, sha256, sha384, sha512 (default: sha256)");
        AnsiConsole.MarkupLine("  --log\t\tpath to log file (default: stdout)");
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();

        // Examples
        AnsiConsole.MarkupLine("[magenta1]Examples:[/]");
        AnsiConsole.MarkupLine("  bagit.net.cli.exe [cyan]create[/] [darkorange]--algorithm md5[/] [red]<directory>[/]");
        AnsiConsole.MarkupLine("  bagit.net.cli.exe [cyan]create[/] [darkorange]--log bagit.net.log[/] [red]<directory>[/]");

        return 0;
    }

}