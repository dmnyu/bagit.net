using bagit.net.interfaces;
using bagit.net.domain;
using System.Text.RegularExpressions;


namespace bagit.net.services
{
    public class ValidationService : IValidationService
    {
        readonly ITagFileService _tagFileService;
        readonly IManifestService _manifestService;
        readonly IMessageService _messageService;
        public const string ManifestPattern = @"manifest-(md5|sha1|sha256|sha384|sha512).txt";
        public ValidationService(ITagFileService tagFileService, IManifestService manifestService, IMessageService messageService) 
        {
            _tagFileService = tagFileService;
            _manifestService = manifestService;
            _messageService = messageService;
        }

        public bool HasRequiredFiles(string bagPath)
        {

            var rootFiles = Directory.EnumerateFiles(bagPath);

            //at minimum, a bag needs a bagit.txt, a 'data' directory and at least one manifest file

            //check for a bagit.txt
            if (!rootFiles.Contains(Path.Combine(bagPath, "bagit.txt"))) {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"bagit.txt is missing from the {bagPath}."));
                return false;
            }

            //check for a data dir
            if (!Directory.Exists(Path.Combine(bagPath, "data"))) 
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"data directory missing from bag root"));
            }

            //check that at least one manifest exists
            var manifestRegex = new Regex(ManifestPattern);
            var tagmanifests = new List<string>();

            var hasManifest = false;
            foreach (var file in rootFiles)
            {
                if (manifestRegex.IsMatch(file))
                {
                    hasManifest = true;
                    break;
                }
            }

            if (!hasManifest)
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, "bag does not contain any manifest files."));
                return false;
            }
            
            return true;
        }

        public void ValidateBag(string bagPath)
        {
            //Check that all required files  
            if(!HasRequiredFiles(bagPath))
                return;

            //must have a valid bagit.txt
            _tagFileService.ValidateBagitTXT(bagPath);

            //if there is a baginfo.txt
            var _bagInfo = Path.Combine(bagPath, "bag-info.txt");
            if(!File.Exists(_bagInfo))
            {
                _messageService.Add(new MessageRecord(MessageLevel.WARNING, ($"{bagPath} does not contain a bag-info.txt file")));
            }
            else
            {
                _tagFileService.ValidateBagInfo(_bagInfo);
            }

            //validate any manifests
            _manifestService.ValidateManifestFiles(bagPath);

        }

        public void ValidateBagFast(string bagPath)
        {
            if (!_tagFileService.HasBagInfo(bagPath))
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"{bagPath} does not contain a bag-info.txt file"));
                return;
            }

            var tags = _tagFileService.GetTags(Path.Combine(bagPath, "bag-info.txt"));
            if (!tags.TryGetValue("Payload-Oxum", out var storedOxumTag))
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"bag-info.txt does not contain a Payload-Oxum tag"));
                return;
            }

            if (storedOxumTag.Count > 1)
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"bag-info.txt contains multiple Payload-Oxum tags"));
                return;
            }
            var storedOxum = storedOxumTag[0];
            var actualOxum = _tagFileService.CalculateOxum(bagPath);
            if (!string.Equals(storedOxum, actualOxum, StringComparison.Ordinal))
            {
                //2025-12-04 13:29:12,305 - ERROR - .\bag-invalid-oxum\ is invalid: Payload-Oxum validation failed. Expected 1 files and 6 bytes but found 1 files and 5 bytes
                var directoryName = Path.GetFileName(bagPath); 
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, ($"{directoryName} is invalid: Payload-Oxum validation failed, expected {storedOxum} found {actualOxum}")));
                return;
            }

            _messageService.Add(new MessageRecord(MessageLevel.INFO, "bag is valid according to payload oxum"));
        }

        public void ValidateBagCompleteness(string bagPath)
        {
            _manifestService.ValidateManifestFilesCompleteness(bagPath);
        }

    }
}
