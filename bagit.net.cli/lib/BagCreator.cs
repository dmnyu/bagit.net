using bagit.net.domain;
using bagit.net.interfaces;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Security.Cryptography;

namespace bagit.net.cli.lib
{
    public class BagCreator
    {
        readonly ICreationService _creationService;
        readonly ILogger _logger;
        readonly IMessageService _messageService;
            
        public BagCreator(ILogger<BagCreator> logger, ICreationService creationService, IMessageService messageService)
        {
            _logger = logger;
            _creationService = creationService;
            _messageService = messageService;
        }

        public int CreateBag(string? dirLocation, string? checkSumAlgorithm, string? tagFile, bool quiet, string? logFile, CancellationToken cancellationToken)
        {
            _messageService.Add(new MessageRecord(MessageLevel.INFO, $"using bagit.net v{Bagit.VERSION}"));
            if (string.IsNullOrWhiteSpace(dirLocation))
            {
                AnsiConsole.MarkupLine("[red][bold]ERROR:[/][/]");
                AnsiConsole.MarkupLine("[red]a directory to bag must be specified when creating a bag[/]\n");
                BagitCLI.app.Run(new string[] { "help" }, cancellationToken);
                return 1;
            }

            var bagPath = Path.GetFullPath(dirLocation);
            if (!Directory.Exists(bagPath))
            {
                AnsiConsole.MarkupLine("[red][bold]ERROR:[/][/]");
                AnsiConsole.MarkupLine($"[red]the directory {bagPath} does not exist[/]\n");
                BagitCLI.app.Run(new string[] { "help" }, cancellationToken);
                return 1;
            }


            //get the algorithms
            IEnumerable<ChecksumAlgorithm> algorithms;
            if (string.IsNullOrWhiteSpace(checkSumAlgorithm))
            {
                algorithms = new List<ChecksumAlgorithm>() { ChecksumAlgorithm.SHA256 };
            }
            else
            {
                algorithms = GetAlgorithms(checkSumAlgorithm);
            }

            if (!algorithms.Any())
            {
                AnsiConsole.MarkupLine("[red][bold]ERROR:[/][/]");
                AnsiConsole.MarkupLine($"[red]no supported checksum algorithms found[/]\n");
                BagitCLI.app.Run(new string[] { "help" }, cancellationToken);
                return 1;
            }


            //get logging option
            if (!string.IsNullOrWhiteSpace(logFile))
            {
                AnsiConsole.MarkupLine($"bagit.net.cli v{Bagit.VERSION}");
                AnsiConsole.MarkupLine($"Logging to {logFile}");
            }

            
            _creationService.CreateBag(bagPath, algorithms, tagFile);
            var messages = _messageService.GetAll();
            Logging.LogEvents(messages, quiet, _logger);
            
            return 0;
        }

        private IEnumerable<ChecksumAlgorithm> GetAlgorithms(string algorithmCmd)
        {
            var algorithms = new List<ChecksumAlgorithm>();
            var algorithmSplit = algorithmCmd.Split(",");
            if (algorithmSplit.Length == 0) {
                var ca = algorithmCmd.ToLower().Trim();
                if (ChecksumAlgorithmMap.Algorithms.ContainsKey(ca))
                {
                    algorithms.Add(ChecksumAlgorithmMap.Algorithms[ca]);
                }
            }
            foreach(var candidateAlgorithm in algorithmSplit)
            {
                var ca = candidateAlgorithm.ToLower().Trim();
                if (ChecksumAlgorithmMap.Algorithms.ContainsKey(ca))
                {
                    algorithms.Add(ChecksumAlgorithmMap.Algorithms[ca]);
                }

            }
            return algorithms;
        }
    }
}
