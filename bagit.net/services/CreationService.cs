using bagit.net.domain;
using bagit.net.interfaces;
using Microsoft.Extensions.Logging;

namespace bagit.net.services
{
    public class CreationService : ICreationService
    {
        readonly ILogger _logger;
        readonly ITagFileService _tagFileService;
        readonly IManifestService _manifestService;
        readonly IFileManagerService _fileManagerService;
        public CreationService(ILogger<CreationService> logger, ITagFileService tagFileService, IManifestService manifestService, IFileManagerService fileManagerService)
        {
            _logger = logger;
            _tagFileService = tagFileService;
            _manifestService = manifestService;
            _fileManagerService = fileManagerService;
        }
        public void CreateBag(string dirLocation, ChecksumAlgorithm algorithm)
        {
            _logger.LogInformation($"Using bagit.net v{Bagit.VERSION}");
            if (dirLocation == null)
            {
                _logger.LogCritical("The path to a directory cannot be null");
                throw new ArgumentNullException(nameof(dirLocation), "The path to a directory cannot be null");
            }
            if (!Directory.Exists(dirLocation))
            {
                _logger.LogCritical("{path} does not exist", dirLocation);
                throw new DirectoryNotFoundException($"{dirLocation} does not exist");
            }

            _logger.LogInformation("Creating bag for directory {path}", dirLocation);

            var dataDir = Path.Combine(dirLocation, "data");
            try
            {
                _logger.LogInformation($"Creating data directory");
                var tempDataDir = _fileManagerService.CreateTempDirectory(dirLocation);
                _fileManagerService.MoveContentsOfDirectory(dirLocation, tempDataDir);
                _logger.LogInformation("Moving {tempDataDir} to data", tempDataDir);
                _fileManagerService.MoveDirectory(tempDataDir, Path.Combine(dirLocation, "data"));
                _manifestService.CreatePayloadManifest(dirLocation, algorithm);
                _logger.LogInformation("Creating bagit.txt");
                _tagFileService.CreateBagItTXT(dirLocation);
                _logger.LogInformation("Creating bag-info.txt");
                _tagFileService.CreateBagInfo(dirLocation);
                _manifestService.CreateTagManifestFile(dirLocation, algorithm);
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex, "Failed to create bag at {Path}", dirLocation);
                throw new InvalidOperationException($"Failed to create bag at {dirLocation}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogCritical(ex, "Access denied when creating bag at {path}", dirLocation);
                throw new InvalidOperationException($"Access denied when creating bag at {dirLocation}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unknown exception while creating bag at {path}", dirLocation);
                throw new InvalidOperationException($"Unknown exception while creating bag at {dirLocation}", ex);
            }
        }
    }
}
