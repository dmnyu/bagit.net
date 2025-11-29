using bagit.net.domain;
using bagit.net.interfaces;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace bagit.net.cli
{
    public class Bagger
    {
        readonly ICreationService _creationService;
        readonly ILogger _logger;
            
        public Bagger(ILogger<Validator> logger, ICreationService creationService)
        {
            _logger = logger;
            _creationService = creationService;
        }

        public int CreateBag(string? dirLocation, string? checkSumAlgorithm, string? logFile, CancellationToken cancellationToken)
        {
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

            //get the algorithm
            ChecksumAlgorithm algorithm;
            if (string.IsNullOrWhiteSpace(checkSumAlgorithm))
            {
                algorithm = ChecksumAlgorithm.SHA256;
            }
            else if (ChecksumAlgorithmMap.Algorithms.ContainsKey(checkSumAlgorithm))
            {
                algorithm = ChecksumAlgorithmMap.Algorithms[checkSumAlgorithm];
            }
            else
            {
                AnsiConsole.MarkupLine("[red][bold]ERROR:[/][/]");
                AnsiConsole.MarkupLine($"[red]checksum algorithm {checkSumAlgorithm} is not supported[/]\n");
                BagitCLI.app.Run(new string[] { "help" }, cancellationToken);
                return 1;
            }

            //get logging option
            if (!string.IsNullOrWhiteSpace(logFile))
            {
                AnsiConsole.MarkupLine($"bagit.net.cli v{Bagit.VERSION}");
                AnsiConsole.MarkupLine($"Logging to {logFile}");
            }

            _creationService.CreateBag(dirLocation, algorithm);
            return 0;
        }
    }
}
