using bagit.net.cli.lib;
using bagit.net.domain;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace bagit.net.cli.Commands
{
    class ValidateCommand : AsyncCommand<ValidateCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("--log")]
            public string? logFile { get; set; }

            [CommandOption("--fast")]
            public bool Fast { get; set; }

            [CommandOption("--complete")]
            public bool Completeness { get; set; }

            [CommandOption("--quiet")]
            public bool Quiet { get; set; }

            [CommandOption("--processes")]
            public int? Processes { get; set; }

            [CommandArgument(0, "[directory]")]
            [Description("Path to the bag directory to validate.")]
            public required string Directory { get; set; }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            MessageContext.Quiet.Value = settings.Quiet;
            try
            {
                var serviceProvider = ServiceConfigurator.BuildServiceProvider<BagValidator>(settings.logFile);
                var validator = serviceProvider.GetRequiredService<BagValidator>();
                return await validator.ValidateBag(
                    settings.Directory, 
                    settings.Fast, 
                    settings.Completeness, 
                    settings.Quiet, 
                    settings.logFile,
                    settings.Processes,
                    cancellationToken);
            }
            catch (Exception ex) {
                AnsiConsole.MarkupLine($"[red][bold]ERROR:[/] {ex.Message}");
                return 1;
            }
        }
    }
}


