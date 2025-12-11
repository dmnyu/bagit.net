using bagit.net.domain;
using bagit.net.interfaces;
using bagit.net.services;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Security.Cryptography;

namespace bagit.net.cli.lib
{
    public class BagCreator
    {
        readonly ICreationService _creationService;
        readonly ILogger _logger;
        private IMessageService _messageService;
        readonly IManifestService _manifestService;
            
        public BagCreator(ILogger<BagCreator> logger, ICreationService creationService, IMessageService messageService, IManifestService manifestService)
        {
            _logger = logger;
            _creationService = creationService;
            _messageService = messageService;
            _manifestService = manifestService;
        }

        public async Task<int> CreateBag(string? dirLocation, string? checkSumAlgorithm, string? tagFile, string? logFile, int? processes, CancellationToken cancellationToken)
        {
            //_messageService.Add(new MessageRecord(MessageLevel.INFO, $"using bagit.net v{Bagit.VERSION}"));
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

            int p = processes ?? 1;

            try
            {
                await _creationService.CreateBag(bagPath, algorithms, tagFile, p);
            }
            catch (Exception ex)
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"Bag Creation failed: {ex}"));
            }
            
            return 0;
        }

        private IEnumerable<ChecksumAlgorithm> GetAlgorithms(string algorithmCmd)  //move to domain package in core
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
