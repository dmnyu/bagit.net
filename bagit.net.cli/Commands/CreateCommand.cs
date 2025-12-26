using bagit.net.cli.lib;
using bagit.net.domain;
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
       
        Options.Quiet.Value = settings.Quiet;
        Options.BufferSize.Value = 8;
        Options.ChecksumAlgorithms.Value = GetAlgorithms(settings.Algorithm ?? string.Empty);
        Options.LogFile.Value = settings.LogFile ?? null;
        Options.TagFile.Value = settings.LogFile ?? null;
        Options.Processes.Value = settings.Processes ?? 1;
        Options.Directory.Value = settings.Directory;
        Options.CancellationToken.Value = cancellationToken;

        try
        {
            var serviceProvider = BagitServiceProvider.BuildServiceProvider<BagCreator>(settings.LogFile);

            var creator = serviceProvider.GetRequiredService<BagCreator>();

            return await creator.CreateBag();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red][bold]ERROR:[/] {ex.Message}");
            return 1;
        }
    }

    private IEnumerable<ChecksumAlgorithm> GetAlgorithms(string algorithmCmd)  //move to domain package in core
    {
        var algorithms = new List<ChecksumAlgorithm>();
        if (string.IsNullOrWhiteSpace(algorithmCmd))
        {
            algorithms.Add(ChecksumAlgorithm.SHA256);
        }
        else
        {

            var algorithmSplit = algorithmCmd.Split(",");
            if (algorithmSplit.Length == 0)
            {
                var ca = algorithmCmd.ToLower().Trim();
                if (ChecksumAlgorithmMap.Algorithms.ContainsKey(ca))
                {
                    algorithms.Add(ChecksumAlgorithmMap.Algorithms[ca]);
                }
            }
            else
            {
                foreach (var candidateAlgorithm in algorithmSplit)
                {
                    var ca = candidateAlgorithm.ToLower().Trim();
                    if (ChecksumAlgorithmMap.Algorithms.ContainsKey(ca))
                    {
                        algorithms.Add(ChecksumAlgorithmMap.Algorithms[ca]);
                    }

                }
            }
        }
        return algorithms;
    }
}