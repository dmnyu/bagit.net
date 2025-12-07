using bagit.net.domain;
using bagit.net.interfaces;

namespace bagit.net.services
{
    public class CreationService : ICreationService
    {
        readonly IMessageService _messageService;
        readonly ITagFileService _tagFileService;
        readonly IManifestService _manifestService;
        readonly IFileManagerService _fileManagerService;
        public CreationService(IMessageService messageService, ITagFileService tagFileService, IManifestService manifestService, IFileManagerService fileManagerService)
        {
            _messageService = messageService;
            _tagFileService = tagFileService;
            _manifestService = manifestService;
            _fileManagerService = fileManagerService;
        }
        public void CreateBag(string dirLocation, ChecksumAlgorithm algorithm, string? tagFileLocation = null)
        {
            _messageService.Add(new MessageRecord(MessageLevel.INFO, $"Using bagit.net v{Bagit.VERSION}"));
            if (dirLocation == null)
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, "The path to a directory cannot be null"));
                return;
            }
            if (!Directory.Exists(dirLocation))
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"{dirLocation} does not exist"));
                return;
            }

            _messageService.Add(new MessageRecord(MessageLevel.INFO, $"Creating bag for directory {dirLocation}"));

            var dataDir = Path.Combine(dirLocation, "data");

            _messageService.Add(new MessageRecord(MessageLevel.INFO, "Creating data directory"));
            var tempDataDir = _fileManagerService.CreateTempDirectory(dirLocation);
            _fileManagerService.MoveContentsOfDirectory(dirLocation, tempDataDir);
            _messageService.Add(new MessageRecord(MessageLevel.INFO, $"Moving {tempDataDir} to data"));
            _fileManagerService.MoveDirectory(tempDataDir, Path.Combine(dirLocation, "data"));
            _manifestService.CreatePayloadManifest(dirLocation, algorithm);
            _messageService.Add(new MessageRecord(MessageLevel.INFO, "Creating bagit.txt"));
            _tagFileService.CreateBagItTXT(dirLocation);
            _messageService.Add(new MessageRecord(MessageLevel.INFO, "Creating bag-info.txt"));
            
            _tagFileService.CreateBagInfo(dirLocation, tagFileLocation);
            _manifestService.CreateTagManifestFile(dirLocation, algorithm);
        }
    }
}
