using bagit.net.interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace bagit.net.services
{
    internal class ValidationService : IValidationService
    {
        readonly ILogger<ValidationService> _logger;
        public ValidationService(ILogger<ValidationService> logger) 
        {
            _logger = logger;
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
            throw new NotImplementedException();
        }

        public void ValidateBagFast(string bagPath)
        {
            throw new NotImplementedException();
        }
    }
}
