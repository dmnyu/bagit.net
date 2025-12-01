using bagit.net.cli.lib;
using bagit.net.domain;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace bagit.net.cli.Commands
{
    class ValidateCommand : Command<ValidateCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("--log")]
            public string? logFile { get; set; }

            [CommandOption("--fast")]
            public bool Fast { get; set; }

            [CommandOption("--complete")]
            public bool Completeness { get; set; }

            [CommandArgument(0, "[directory]")]
            [Description("Path to the bag directory to validate.")]
            public string? Directory { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            try
            {
                var serviceProvider = ServiceConfigurator.BuildServiceProvider<Validator>(settings.logFile);
                var validator = serviceProvider.GetRequiredService<Validator>();
                validator.ValidateBag(settings.Directory, settings.Fast, settings.Completeness, settings.logFile, cancellationToken);
            }
            catch (Exception ex) {
                AnsiConsole.MarkupLine($"[red][bold]ERROR:[/] {ex.Message}");
                return 1;
            }
            return 0;
        }
    }
}


