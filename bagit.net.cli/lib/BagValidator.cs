using bagit.net.domain;
using bagit.net.interfaces;
using bagit.net.services;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Formats.Tar;

namespace bagit.net.cli.lib
{
    public class BagValidator
    {
        private readonly ILogger _logger;
        private readonly IValidationService _validationService;
        private readonly IMessageService _messageService;
        public BagValidator(ILogger<BagValidator> logger, IValidationService validationService, IMessageService messageService)
        {
            _logger = logger;
            _validationService = validationService;
            _messageService = messageService;
        }

        public async Task<int> ValidateBag(string? bagPath, bool fast, bool complete, bool quiet, string? logFile, int? processes, CancellationToken cancellationToken)
        {
            _messageService.Add(new MessageRecord(MessageLevel.INFO, $"using bagit.net v{Bagit.VERSION}"));

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

                List<MessageRecord> messages;

                if (complete)
                {
                    _validationService.ValidateBagCompleteness(bagPath);
                    messages = _messageService.GetAll().ToList();

                    if(MessageHelpers.HasError(messages))
                    {
                       messages.Add(new MessageRecord(MessageLevel.ERROR, $"{bagPath} is not complete."));
                    } else
                    {
                        messages.Add(new MessageRecord(MessageLevel.INFO, $"{bagPath} is complete."));
                    }
                }
                else if (fast)
                {
                    _validationService.ValidateBagFast(bagPath);
                    messages = _messageService.GetAll().ToList(); 
                }
                else
                {
                    int p = processes ?? 1;

                    try
                    {
                        await _validationService.ValidateBag(bagPath, p);
                    }
                    catch (Exception ex)
                    {
                        _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"Bag Creation failed: {ex}"));
                    }

                

                }
                
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
