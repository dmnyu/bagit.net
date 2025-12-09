using bagit.net.domain;
using Spectre.Console;
using Spectre.Console.Cli;

namespace bagit.net.cli.Commands;

class HelpCommand : Command<HelpCommand.Settings>
{
    public class Settings : CommandSettings { }

    public override int Execute(CommandContext? context, Settings settings, CancellationToken cancellationToken)
    {
        // Header
        AnsiConsole.MarkupLine($"bagit.net.cli v{Bagit.VERSION}");
        AnsiConsole.MarkupLine("- a BagIt (RFC 8493) tool for creating and validating bags");
        AnsiConsole.WriteLine();

        // Usage
        AnsiConsole.MarkupLine("[lime]USAGE:[/]");
        AnsiConsole.MarkupLine("  bagit.net [cyan][[COMMAND]][/] [darkorange][[OPTIONS]][/]");
        AnsiConsole.WriteLine();

        // Commands
        AnsiConsole.MarkupLine("[cyan]COMMANDS:[/]");
        var table = new Table()
            .HideHeaders()
            .NoBorder()
            .AddColumn(new TableColumn(string.Empty))
            .AddColumn(new TableColumn(string.Empty));

        table.AddRow("[cyan]create[/]", "create a bag from an existing directory");
        table.AddRow("[cyan]validate[/]", "validate an existing bag");
        table.AddRow("[cyan]manage[/]", "manage tag files in an existing bag");
        table.AddRow("[cyan]help[/]", "display the help page");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        // CREATE Command Section
        AnsiConsole.MarkupLine("[cyan]CREATE COMMAND[/]");
        AnsiConsole.MarkupLine("[lime]USAGE:[/]");
        AnsiConsole.MarkupLine("  bagit.net [cyan]create[/] [darkorange][[OPTIONS]][/] [red]<directory>[/]");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[darkorange]OPTIONS:[/]");
        var createOptions = new Table()
            .HideHeaders()
            .NoBorder()
            .AddColumn(new TableColumn(string.Empty))
            .AddColumn(new TableColumn(string.Empty));

        createOptions.AddRow("--algorithm", "checksum algorithm: md5, sha1, sha256, sha384, sha512 (default: sha256)");
        createOptions.AddRow("--log", "path/to/logfile (default: stdout)");
        createOptions.AddRow("--tag-file", "path/to/tag-file (metadata to be added to bag-info.txt on creation");

        AnsiConsole.Write(createOptions);
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[magenta1]EXAMPLES:[/]");
        AnsiConsole.MarkupLine("  bagit.net [cyan]create[/]  [darkorange]--algorithm md5[/]  [red]<directory>[/]");
        AnsiConsole.MarkupLine("  bagit.net [cyan]create[/]  [darkorange]--log bagit.net.log[/]  [red]<directory>[/]");
        AnsiConsole.MarkupLine("  bagit.net [cyan]create[/]  [darkorange]--tag-file path/to/tag-file[/]  [red]<directory>[/]");


        // VALIDATE Command Section
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[cyan]VALIDATE COMMAND[/]");
        AnsiConsole.MarkupLine("[lime]USAGE:[/]");
        AnsiConsole.MarkupLine("  bagit.net [cyan]validate[/] [darkorange][[OPTIONS]][/] [red]<directory>[/]");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[darkorange]OPTIONS:[/]");
        var validateOptions = new Table()
            .HideHeaders()
            .NoBorder()
            .AddColumn(new TableColumn(string.Empty))
            .AddColumn(new TableColumn(string.Empty));

        validateOptions.AddRow("--fast", "validate only the bag's Payload-Oxum");
        validateOptions.AddRow("--log", "path to log file (default: stdout)");

        AnsiConsole.Write(validateOptions);
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();

        // Examples
        AnsiConsole.MarkupLine("[magenta1]EXAMPLES:[/]");
        AnsiConsole.MarkupLine("  bagit.net [cyan]validate[/]  [darkorange]--log bagit.net.log[/] [red]<directory>[/]");
        AnsiConsole.MarkupLine("  bagit.net [cyan]validate[/]  [darkorange]--fast[/]  [red]<directory>[/]");

        //MANAGE Command Section
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[cyan]MANAGE COMMAND[/]");
        AnsiConsole.MarkupLine("[lime]USAGE:[/]");
        AnsiConsole.MarkupLine("  bagit.net [cyan]manage[/] [darkorange][[OPTIONS]][/] [red]<directory>[/]");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[darkorange]OPTIONS:[/]");
        var manageOptions = new Table()
            .HideHeaders()
            .NoBorder()
            .AddColumn(new TableColumn(string.Empty))
            .AddColumn(new TableColumn(string.Empty));
        manageOptions.AddRow("--add", "add a new key and value to bag-info.txt");
        manageOptions.AddRow("--set", "replace the value of a key in bag-info.txt");
        manageOptions.AddRow("--delete", "remove a line from bag-info.txt by the key");
        manageOptions.AddRow("--view", "display the contents of bag-info.txt");
        AnsiConsole.Write(manageOptions);
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();

        // Examples
        AnsiConsole.MarkupLine("[magenta1]EXAMPLES:[/]");
        AnsiConsole.MarkupLine("  bagit.net [cyan]manage[/]  [darkorange]--add \"test-tag=test value\"[/]  [red]<directory>[/]");
        AnsiConsole.MarkupLine("  bagit.net [cyan]manage[/]  [darkorange]--set \"test-tag=new value\"[/]  [red]<directory>[/]");
        AnsiConsole.MarkupLine("  bagit.net [cyan]manage[/]  [darkorange]--delete \"test-tag\"[/]  [red]<directory>[/]");
        AnsiConsole.MarkupLine("  bagit.net [cyan]manage[/]  [darkorange]--view[/]  [red]<directory>[/]");


        return 0;
    }
}
