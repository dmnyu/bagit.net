using bagit.net.interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace bagit.net.services
{
    internal class ValidationService : IValidationService
    {
        readonly ILogger<ValidationService> _logger;
        readonly ITagFileService _tagFileService;
        readonly IManifestService _manifestService;
        public ValidationService(ILogger<ValidationService> logger, ITagFileService tagFileService, IManifestService manifestService) 
        {
            _logger = logger;
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

        public void ValidateBag(string bagPath)
        {
            //must hav a valid bagit.txt
            if (!_tagFileService.HasBagItTXT(bagPath))
                throw new InvalidDataException($"{bagPath} does not contain a bagit.xt");

            //validate bagit.txt
            _tagFileService.ValidateBagitTXT(bagPath);

            //must have a data directory
            if (!Directory.Exists(Path.Combine(bagPath, "data")))
                throw new DirectoryNotFoundException($"{bagPath}");
                
            //if there is a baginfo.txt
            var _bagInfo = Path.Combine(bagPath, "bag-info.txt");
            if (File.Exists(_bagInfo))
            {
                _tagFileService.ValidateBagInfo(_bagInfo);
            }

            //validate any manifests
            _manifestService.ValidateManifestFiles(bagPath);
        }

        public void ValidateBagFast(string bagPath)
        {
            if(!_tagFileService.HasBagInfo(bagPath))
            {
                throw new FileNotFoundException($"{bagPath} does not contain a bag-info.txt file");
            }

            var tags = _tagFileService.GetTags(Path.Combine(bagPath, "bag-info.txt"));
            if (!tags.TryGetValue("Payload-Oxum", out var storedOxumTag))
            {
                throw new InvalidDataException($"bag-info.txt does not contain a payload-oxum");
            }

            var storedOxum = storedOxumTag[0];
            var calculatedOxum = _tagFileService.CalculateOxum(bagPath);
            if (storedOxum != calculatedOxum)
            {
                throw new InvalidDataException($"Payload Oxum did not match, expected {storedOxum} found {calculatedOxum}");
            }


        }
    }
}
