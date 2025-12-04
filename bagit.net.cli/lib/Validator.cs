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
        private readonly IMessageService _messageService;
        public Validator(ILogger<Validator> logger, IValidationService validationService, IMessageService messageService)
        {
            _logger = logger;
            _validationService = validationService;
            _messageService = messageService;
        }

        public int ValidateBag(string? bagPath, bool fast, bool complete, string? logFile, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"using bagit.net v{Bagit.VERSION}");

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
                    _validationService.ValidateBagCompleteness(bagPath);
                    var messages = _messageService.GetAll();
                    messages.ToList().ForEach(message => Logging.LogEvent(message, _logger));
                    if(MessageHelpers.HasError(messages))
                    {
                        Logging.LogEvent(new MessageRecord(MessageLevel.ERROR, $"{bagPath} is not complete."), _logger);
                    } else
                    {
                        Logging.LogEvent(new MessageRecord(MessageLevel.INFO, $"{bagPath} is complete."), _logger);
                    }

                        return 0; // Return non-zero to indicate failure
                }
                else if (fast)
                {
                    _validationService.ValidateBagFast(bagPath);
                    var messages = _messageService.GetAll();
                    messages.ToList().ForEach(message => Logging.LogEvent(message, _logger));
                    return 0;
                }
                else
                {
                    _validationService.ValidateBag(bagPath);
                    var messages = _messageService.GetAll();
                    messages.ToList().ForEach(message => Logging.LogEvent(message, _logger));
                    if(!MessageHelpers.HasError(messages))
                    {
                        Logging.LogEvent(new MessageRecord(MessageLevel.INFO, "bag is valid"), _logger);    
                    }
                    return 0;

                }
            } catch (Exception ex) {
                _logger.LogCritical(ex, "Failed to validate bag at {Path}", bagPath);
                AnsiConsole.MarkupLine("[red][bold]ERROR:[/][/]");
                AnsiConsole.MarkupLine($"[red]Failed to validate bag at {bagPath}[/]\n");
                return 1;
            }
        }

    }
}
