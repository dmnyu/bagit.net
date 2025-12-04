using bagit.net.interfaces;
using bagit.net.domain;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Reflection.Metadata.Ecma335;

namespace bagit.net.services
{
    public class ValidationService : IValidationService
    {
        readonly ITagFileService _tagFileService;
        readonly IManifestService _manifestService;
        public ValidationService(ITagFileService tagFileService, IManifestService manifestService) 
        {
            _tagFileService = tagFileService;
            _manifestService = manifestService;
        }

        public void HasRequiredFiles(string bagPath)
        {
            
            var rootFiles = Directory.EnumerateFiles(bagPath);

            //at minimum, a bag needs a bagit.txt, a 'data' directory and at least one manifest file

            //check for a bagit.txt
            if (!rootFiles.Contains(Path.Combine(bagPath, "bagit.txt")))
                throw new FileNotFoundException("bagit.txt is missing from the bag root.", bagPath);
            
            //check for a data dir
            if (!Directory.Exists(Path.Combine(bagPath, "data")))
                throw new DirectoryNotFoundException("data directory missing from bag root");

            //check that at least one manifest exists
            var manifestRegex = new Regex(ServiceHelpers.ManifestPattern);
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
                throw new FileNotFoundException("bag does not contain a manifest file.", bagPath);
        }

        public IEnumerable<MessageRecord> ValidateBag(string bagPath)
        {
            var messages = new List<MessageRecord>();
            //must have a valid bagit.txt
            if (!_tagFileService.HasBagItTXT(bagPath))
            {
                messages.Add(new MessageRecord(MessageLevel.ERROR, ($"{bagPath} does not contain a bagit.txt file")));
            }
            else
            {
                //validate bagit.txt
                messages.AddRange(_tagFileService.ValidateBagitTXT(bagPath));
            }

            //must have a data directory
            if (!Directory.Exists(Path.Combine(bagPath, "data")))
                messages.Add(new MessageRecord(MessageLevel.ERROR, $"{bagPath} does not contain `data` directory"));

            //if there is a baginfo.txt
            var _bagInfo = Path.Combine(bagPath, "bag-info.txt");
            if(!File.Exists(_bagInfo))
            {
                messages.Add(new MessageRecord(MessageLevel.WARNING, ($"{bagPath} does not contain a bag-info.txt file")));
            }
            else
            {
                messages.AddRange(_tagFileService.ValidateBagInfo(_bagInfo));
            }

            //validate any manifests
            messages.AddRange(_manifestService.ValidateManifestFiles(bagPath));

            return messages; 
        }

        public MessageRecord ValidateBagFast(string bagPath)
        {
            if (!_tagFileService.HasBagInfo(bagPath))
                return new MessageRecord(MessageLevel.ERROR, ($"{bagPath} does not contain a bag-info.txt file"));

            var tags = _tagFileService.GetTags(Path.Combine(bagPath, "bag-info.txt"));
            if (!tags.TryGetValue("Payload-Oxum", out var storedOxumTag))
                return new MessageRecord(MessageLevel.ERROR, ($"bag-info.txt does not contain a payload-oxum"));

            var storedOxum = storedOxumTag[0];
            var actualOxum = _tagFileService.CalculateOxum(bagPath);
            if (!string.Equals(storedOxum, actualOxum, StringComparison.Ordinal))
                return new MessageRecord(MessageLevel.ERROR, ($"Payload Oxum did not match, expected {storedOxum} found {actualOxum}"));

            return new MessageRecord(MessageLevel.INFO, "bag is valid according to payload oxum");
        }

        public IEnumerable<MessageRecord> ValidateBagCompleteness(string bagPath)
        {
            return _manifestService.ValidateManifestFilesCompleteness(bagPath);
        }
    }
}
