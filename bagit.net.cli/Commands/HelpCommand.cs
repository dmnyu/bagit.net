using Spectre.Console;
using Spectre.Console.Cli;
using System.IO;

namespace bagit.net.cli.Commands;

class HelpCommand : Command<HelpCommand.Settings>
{
    public class Settings : CommandSettings
    {

    }
    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLine($"bagit.net v{Bagit.VERSION}");
        AnsiConsole.MarkupLine("\n[bold][lime]USAGE:[/][/]");
        AnsiConsole.MarkupLine("bagit.net.cli.exe [cyan][[COMMAND]][/] [darkorange][[OPTIONS]][/]");
        AnsiConsole.MarkupLine("\n[yellow]COMMANDS:[/]");
        AnsiConsole.MarkupLine("  [cyan]create[/]\tcreate a bag from an existing directory"); 
        AnsiConsole.MarkupLine("  [cyan]help[/]\t\tdisplay the help page");
        AnsiConsole.MarkupLine("\n  [cyan][bold]CREATE[/][/]");
        AnsiConsole.MarkupLine("[lime]  USAGE:[/] bagit.net.cli.exe [cyan]create[/] [darkorange][[OPTIONS]][/] [red]<Directory>[/]");
        AnsiConsole.MarkupLine("\n  [darkorange]OPTIONS[/]");
        List<string> args = new List<string>() { "md5", "sha1", "sha256", "sha384", "sha512" };
        AnsiConsole.MarkupLine($"    --log\tpath to log file (default: stdout)");
        AnsiConsole.MarkupLine($"\n    [grey]checksum algorithms (default: sha256):[/]");
        foreach (string arg in args)
        {
            AnsiConsole.MarkupLine($"    --{arg}\tgenerate manifests with {arg} checksums when creating a bag");
        }
        AnsiConsole.MarkupLine("\n[bold][magenta1]Examples:[/][/]");
        AnsiConsole.MarkupLine("  bagit.net.cli.exe create --md5 [red]<directory>[/]");
        AnsiConsole.MarkupLine("  bagit.net.cli.exe create --md5 --log bagit.net.log [red]<directory>[/]");
        return 0;
    }
}