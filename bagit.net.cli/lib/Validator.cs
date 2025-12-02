using bagit.net.domain;
using bagit.net.interfaces;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace bagit.net.cli.lib
{
    public class Validator
    {
        private readonly ILogger _logger;
        private readonly IValidationService _validationService;
        public Validator(ILogger<Validator> logger, IValidationService validationService)
        {
            _logger = logger;
            _validationService = validationService;
        }
        public int ValidateBag(string? bagPath, bool fast, bool complete, string? logFile, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Using bagit.net v{Bagit.VERSION}");

            if (fast && complete)
            {
                AnsiConsole.MarkupLine("[red][bold]ERROR:[/][/]");
                AnsiConsole.MarkupLine($"[red]validation can only be run with one of --fast and --complete flags set[/]\n");
                return 1;
            }

            try
            {

                if (string.IsNullOrWhiteSpace(bagPath))
                {
                    AnsiConsole.MarkupLine("[red][bold]ERROR:[/][/]");
                    AnsiConsole.MarkupLine("[red]a directory to a BagIt bag must be specified when creating a bag[/]\n");
                    BagitCLI.app.Run(new string[] { "help" }, cancellationToken);
                    return 1;
                }

                bagPath = Path.GetFullPath(bagPath);
                if (!Directory.Exists(bagPath))
                {
                    AnsiConsole.MarkupLine("[red][bold]ERROR:[/][/]");
                    AnsiConsole.MarkupLine($"[red]the directory {bagPath} does not exist[/]\n");
                    BagitCLI.app.Run(new string[] { "help" }, cancellationToken);
                    return 1;
                }

                if (!string.IsNullOrWhiteSpace(logFile))
                {
                    AnsiConsole.MarkupLine($"bagit.net.cli v{Bagit.VERSION}");
                    AnsiConsole.MarkupLine($"Logging to {logFile}");
                }

                if (complete)
                {
                    var messages = _validationService.ValidateBagCompleteness(bagPath);
                    if (messages.Count() == 0)
                    {
                        _logger.LogInformation($"{bagPath} is complete according to manifests files");
                        return 0;
                    }

                    foreach(var message  in messages)
                    {
                        Logging.LogEvent(message, _logger);
                    }
                    _logger.LogError($"{bagPath} is not complete according to manifests files");
                    return 0;
                }

                if (fast)
                {  
                    _validationService.ValidateBagFast(bagPath);
                    _logger.LogInformation($"{bagPath} is valid according to Payload-Oxum");
                    return 0;
                }

                _validationService.ValidateBag(bagPath);
                _logger.LogInformation($"{bagPath} is valid");
                return 0;
            } catch (Exception ex) {
                _logger.LogCritical(ex, "Failed to validate bag at {Path}", bagPath);
                AnsiConsole.MarkupLine("[red][bold]ERROR:[/][/]");
                AnsiConsole.MarkupLine($"[red]Failed to validate bag at {bagPath}[/]\n");
                return 1;
            }
        }

    }
}
