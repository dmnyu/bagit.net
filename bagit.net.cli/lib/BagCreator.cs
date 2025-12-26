using bagit.net.domain;
using bagit.net.interfaces;
using bagit.net.services;
using Microsoft.Extensions.Logging;
using Spectre.Console;


namespace bagit.net.cli.lib
{
    public class BagCreator
    {
        private readonly ICreationService _creationService;
        private readonly IMessageService _messageService;

        public BagCreator(ICreationService creationService, IMessageService messageService)
        {
            _creationService = creationService;
            _messageService = messageService;
        }

        public async Task<int> CreateBag()
        {
            //_messageService.Add(new MessageRecord(MessageLevel.INFO, $"using bagit.net v{Bagit.VERSION}"));
            if (string.IsNullOrWhiteSpace(Options.Directory.Value))
            {
                AnsiConsole.MarkupLine("[red][bold]ERROR:[/][/]");
                AnsiConsole.MarkupLine("[red]a directory to bag must be specified when creating a bag[/]\n");
                BagitCLI.app.Run(new string[] { "help" }, Options.CancellationToken.Value);
                return 1;
            }

            Options.Directory.Value = Path.GetFullPath(Options.Directory.Value);
            if (!Directory.Exists(Options.Directory.Value))
            {
                AnsiConsole.MarkupLine("[red][bold]ERROR:[/][/]");
                AnsiConsole.MarkupLine($"[red]the directory {Options.Directory.Value} does not exist[/]\n");
                BagitCLI.app.Run(new string[] { "help" }, Options.CancellationToken.Value);
                return 1;
            }


            //get logging option
            if (!string.IsNullOrWhiteSpace(Options.LogFile.Value))
            {
                AnsiConsole.MarkupLine($"bagit.net.cli v{Bagit.VERSION}");
                AnsiConsole.MarkupLine($"Logging to {Options.LogFile.Value}");
            }

            try
            {
                await _creationService.CreateBag(Options.Directory.Value, Options.ChecksumAlgorithms.Value, Options.TagFile.Value, Options.Processes.Value);
            }
            catch (Exception ex)
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"Bag Creation failed: {ex}"));
            }
            
            return 0;
        }
    }
}
