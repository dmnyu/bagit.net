using bagit.net.services;
using Microsoft.Extensions.Logging;
using System.Text;

namespace bagit.net
{
    public class Bagger
    {
        private string bagLocation = string.Empty;
        private string dataDir = string.Empty;
        private string tempDataDir = string.Empty;
        private string nl = Environment.NewLine;
        private readonly ILogger _logger;
        private readonly IManifestService _manifestService;
        private readonly IBagInfoService _bagInfoService;
            
        public Bagger(ILogger<Validator> logger, IManifestService manifestService, IBagInfoService bagInfoService)
        {
            _logger = logger;
            _manifestService = manifestService;
            _bagInfoService = bagInfoService;
        }

        public void CreateBag(string? path, ChecksumAlgorithm algorithm)
        {
            _logger.LogInformation($"Using bagit.net v{Bagit.VERSION}");
            if (path == null)
            {
                _logger.LogCritical("The path to a directory cannot be null");
                throw new ArgumentNullException(nameof(path),"The path to a directory cannot be null");
            }
            if (!Directory.Exists(path))
            {
                _logger.LogCritical("{path} does not exist", path);
                throw new DirectoryNotFoundException($"{path} does not exist");
            }

            _logger.LogInformation("Creating bag for directory {path}", path);
            bagLocation = path;
            dataDir = Path.Combine(bagLocation, "data");
            try
            {
                CreateTempDataDir();
                MoveContentsToTemp();
                MoveTempToDataDir();
                _manifestService.CreatePayloadManifest(bagLocation, algorithm);
                CreateBagitTXT();
               _bagInfoService.CreateBagInfo(bagLocation);
                _manifestService.CreateTagManifestFile(bagLocation, algorithm);
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex, "Failed to create bag at {Path}", path);
                throw new InvalidOperationException($"Failed to create bag at {path}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogCritical(ex, "Access denied when creating bag at {path}", path);
                throw new InvalidOperationException($"Access denied when creating bag at {path}", ex);
            }
            catch (Exception ex){
                _logger.LogCritical(ex, "Unknown exception while creating bag at {path}", path);
                throw new Exception($"Unknown exception while creating bag at {path}", ex);
            }
        }

        internal void CreateTempDataDir()
        {
            _logger.LogInformation($"Creating {dataDir}");
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var suffix = new string(Enumerable.Range(0, 8)
                                             .Select(_ => chars[random.Next(chars.Length)])
                                             .ToArray());

            tempDataDir = Path.Combine(bagLocation, $"tmp{suffix}");
            Directory.CreateDirectory(tempDataDir);
        }

        internal void MoveContentsToTemp()
        {
            // Move all top-level files
            foreach (var file in Directory.GetFiles(bagLocation))
            {
                var filename = Path.GetFileName(file);
                var dest = Path.Combine(tempDataDir, filename);
                _logger.LogInformation("Moving {filename} to {dest}", filename, dest);
                File.Move(file, dest);
            }

            // Move all top-level directories except the temp folder itself
            foreach (var dir in Directory.GetDirectories(bagLocation))
            {
                if (dir.Equals(tempDataDir, StringComparison.OrdinalIgnoreCase))
                    continue;

                var dirName = Path.GetFileName(dir);
                var destDir = Path.Combine(tempDataDir, dirName);
                _logger.LogInformation("Moving {dirName} to {destDir}", dirName, destDir);
                Directory.Move(dir, destDir);
            }
        }

        internal void MoveTempToDataDir()
        {
            var tmpName = Path.GetFileName(tempDataDir);
            _logger.LogInformation("Moving {tempDataDir} to data", tempDataDir);
            Directory.Move(tempDataDir, dataDir);
        }

        internal void CreateBagitTXT()
        {
            _logger.LogInformation("Creating bagit.txt");
            var bagitTxt = Path.Combine(bagLocation, "bagit.txt");
            if (!System.Text.RegularExpressions.Regex.IsMatch(Bagit.BAGIT_VERSION, @"^\d+\.\d+$"))
            {
                _logger.LogCritical("Invalid BagIt version: {b}. Must be in 'major.minor' format.", Bagit.BAGIT_VERSION);
                throw new InvalidOperationException($"Invalid BagIt version: {Bagit.BAGIT_VERSION}. Must be in 'major.minor' format.");
            }
            var content = $"BagIt-Version: {Bagit.BAGIT_VERSION}{nl}Tag-File-Character-Encoding: UTF-8{nl}";
            File.WriteAllText(bagitTxt, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
    }
}
