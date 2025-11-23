using bagit.net.services;
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

        if (string.IsNullOrWhiteSpace(settings.Directory))
        {
            AnsiConsole.MarkupLine("[red][bold]ERROR:[/][/]");
            AnsiConsole.MarkupLine("[red]a directory to bag must be specified when creating a bag[/]\n");
            BagitCLI.app.Run(new string[] { "help" }, cancellationToken);
            return 1;
        }

        var bagPath = Path.GetFullPath(settings.Directory);
        if (!Directory.Exists(bagPath)) {
            AnsiConsole.MarkupLine("[red][bold]ERROR:[/][/]");
            AnsiConsole.MarkupLine($"[red]the directory {bagPath} does not exist[/]\n");
            BagitCLI.app.Run(new string[] { "help" }, cancellationToken);
            return 1;
        }

        //get the algorithm
        ChecksumAlgorithm algorithm;
        if (string.IsNullOrWhiteSpace(settings.Algorithm))
        {
            algorithm = ChecksumAlgorithm.SHA256;
        } else if(Bagit.Algorithms.ContainsKey(settings.Algorithm))
        {
            algorithm = Bagit.Algorithms[settings.Algorithm];
        } else
        {
            AnsiConsole.MarkupLine("[red][bold]ERROR:[/][/]");
            AnsiConsole.MarkupLine($"[red]checksum algorithm {settings.Algorithm} is not supported[/]\n");
            BagitCLI.app.Run(new string[] { "help" }, cancellationToken);
            return 1;
        }

        //get logging option
        if (!string.IsNullOrWhiteSpace(settings.logFile))
        {
            AnsiConsole.MarkupLine($"bagit.net.cli v{Bagit.VERSION}");
            AnsiConsole.MarkupLine($"Logging to {settings.logFile}");
        }

        var serviceProvider = ServiceConfigurator.BuildServiceProvider<Bagger>();
        var bagger = serviceProvider.GetRequiredService<Bagger>();
        bagger.CreateBag(bagPath, algorithm);
        return 0;
    }
}