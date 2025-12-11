using bagit.net.cli.lib;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace bagit.net.cli.Commands;

public class CreateCommand : AsyncCommand<CreateCommand.Settings>
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

        [CommandOption("--processes")]
        public int? Processes { get; set; } 

        [CommandArgument(0, "[directory]")]
        [Description("Path to the directory to bag.")]
        required public string Directory { get; set; }

    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var serviceProvider = ServiceConfigurator
                .BuildServiceProvider<BagCreator>(settings.LogFile);

            var creator = serviceProvider.GetRequiredService<BagCreator>();

            return await creator.CreateBag(
                settings.Directory,
                settings.Algorithm,
                settings.TagFile,
                settings.LogFile,
                settings.Processes,
                cancellationToken);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red][bold]ERROR:[/] {ex.Message}");
            return 1;
        }
    }
}