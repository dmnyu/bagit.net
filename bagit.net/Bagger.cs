using bagit.net.services;
using bagit.net.interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace bagit.net
{
    public class Bagger
    {
        private string bagLocation = string.Empty;
        private string dataDir = string.Empty;
        private string tempDataDir = string.Empty;
        private readonly ILogger _logger;
        private readonly IManifestService _manifestService;
        private readonly ITagFileService _tagFileService;
        private readonly IFileManagerService _fileManagerService;
            
        public Bagger(ILogger<Bagger> logger, IManifestService manifestService, ITagFileService tagFileService, IFileManagerService fileManagerService  )
        {
            _logger = logger;
            _manifestService = manifestService;
            _tagFileService = tagFileService;
            _fileManagerService = fileManagerService;
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
                _logger.LogInformation($"Creating {dataDir}");
                tempDataDir = _fileManagerService.CreateTempDirectory(bagLocation);
                _fileManagerService.MoveContentsOfDirectory(bagLocation, tempDataDir);
                _logger.LogInformation("Moving {tempDataDir} to data", tempDataDir);
                _fileManagerService.MoveDirectory(tempDataDir, Path.Combine(bagLocation, "data"));
                _manifestService.CreatePayloadManifest(bagLocation, algorithm);
                _tagFileService.CreateBagItTXT(bagLocation);
                _tagFileService.CreateBagInfo(bagLocation);
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

    }
}
