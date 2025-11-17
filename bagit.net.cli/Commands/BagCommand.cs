using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace bagit.net.cli.Commands;

class BagCommand : Command<BagCommand.Settings>
{
    public class Settings : CommandSettings
    {

        [CommandOption("--md5")]
        public bool Md5 { get; set; }

        [CommandOption("--sha1")]
        public bool Sha1 { get; set; }

        [CommandOption("--sha256")]
        public bool Sha256 { get; set; }

        [CommandOption("--sha384")]
        public bool Sha384 { get; set; }

        [CommandOption("--sha512")]
        public bool Sha512 { get; set; }

        [CommandOption("--log")]
        public string logFile { get; set; }

        [CommandArgument(0, "<directory>")]
        [Description("Path to the directory to bag.")]
        public string Directory { get; set; } = null!;

    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var bagPath = Path.GetFullPath(settings.Directory);
        //AnsiConsole.MarkupLine($"[yellow]Creating bag for directory[/] {bagPath}");
        var algorithms = new List<ChecksumAlgorithm>();
        if (settings.Md5) algorithms.Add(ChecksumAlgorithm.MD5);
        if (settings.Sha1) algorithms.Add(ChecksumAlgorithm.SHA1);
        if (settings.Sha256) algorithms.Add(ChecksumAlgorithm.SHA256);
        if (settings.Sha384) algorithms.Add(ChecksumAlgorithm.SHA384);
        if (settings.Sha512) algorithms.Add(ChecksumAlgorithm.SHA512);
        if (algorithms.Count == 0)
        {
            algorithms.Add(ChecksumAlgorithm.SHA256);
        }

        Bagit.InitLogger(settings.logFile);
        Bagit.Logger.LogInformation($"Using bagit.net v{Bagit.VERSION}");
        var bagger = new Bagger();
        bagger.CreateBag(bagPath, algorithms[0]);
        return 0;
    }
}