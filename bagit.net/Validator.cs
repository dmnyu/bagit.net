using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;


namespace bagit.net
{
    public class Validator
    {
        private readonly ILogger _logger;
        private readonly IManifestService _manifestService;
        private readonly IBagInfoService _bagInfoService;
        public Validator(ILogger<Validator> logger, IManifestService manifestService, IBagInfoService bagInfoService)
        {
            _logger = logger;
            _manifestService = manifestService;
            _bagInfoService = bagInfoService;
        }
        public void ValidateBag(string bagPath, bool fast)
        {
            _logger.LogInformation($"Using bagit.net v{Bagit.VERSION}");
            try
            {   
                if(fast)
                {  
                    Has_Valid_BaginfoTXT(bagPath, fast);
                    _logger.LogInformation($"{bagPath} is valid according to Payload-Oxum");
                    return;
                }
                Has_Required_Files(bagPath);
                Has_Valid_BagitTXT(bagPath);
                Has_Valid_BaginfoTXT(bagPath, fast);
                ValidateManifests(bagPath);
                _logger.LogInformation($"{bagPath} is valid");
            } catch (Exception ex) {
                _logger.LogCritical(ex, "Failed to validate bag at {Path}", bagPath);
                throw;
            }
        }

        internal void Has_Required_Files(string bagPath)
        {
            var files = Directory.EnumerateFiles(bagPath);
            
            if (!files.Contains(Path.Combine(bagPath, "bagit.txt")))
                throw new FileNotFoundException("bagit.txt is missing from the bag root.", bagPath);
            if (!Directory.Exists(Path.Combine(bagPath, "data")))
                throw new DirectoryNotFoundException("data directory missing from bag root");
            var bagInfoPath = Path.Combine(bagPath, "bag-info.txt");
            if (!File.Exists(bagInfoPath))
                _logger.LogWarning("bag-info.txt is missing from bag root");

            var manifestRegex = new Regex(Bagit.ManifestPattern);
            var tagmanifestRegex = new Regex(Bagit.TagmanifestPattern);
            var manifests = new List<string>{ };
            var tagmanifests = new List<string>();

            foreach (var file in files)
            {
                if (manifestRegex.IsMatch(file))
                    manifests.Add(file);
                if (tagmanifestRegex.IsMatch(file))
                    tagmanifests.Add(file);
            }

            if (manifests.Count < 1)
                throw new FileNotFoundException("bag does not contain a manifest file.", bagPath);

            var allowedFiles = new List<string>();
            allowedFiles.AddRange(manifests);
            allowedFiles.AddRange(tagmanifests);
            allowedFiles.Add(Path.Combine(bagPath, "bagit.txt"));
            allowedFiles.Add(Path.Combine(bagPath, "bag-info.txt"));

            foreach (var file in files)
            {
                if(!allowedFiles.Contains(file))
                {
                    var fn = Path.GetFileName(file);
                    _logger.LogWarning($"bag contains extra file in root {fn}");
                }
            }
        }

        internal void Has_Valid_BagitTXT(string path)
        {
            var bagitPath = Path.Combine(path, "bagit.txt");
            if (!File.Exists(bagitPath))
                throw new FileNotFoundException("bagit.txt is missing from the bag root.", bagitPath);

            var tags = TagFile.GetTagFileAsDict(bagitPath);

            if (!tags.TryGetValue("BagIt-Version", out var version))
                throw new FormatException("BagIt-Version key is missing in bagit.txt.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(version, @"^\d+\.\d+$"))
                throw new FormatException($"Invalid BagIt-Version format: {version}");

            if (!tags.TryGetValue("Tag-File-Character-Encoding", out var encoding))
                throw new FormatException("Tag-File-Character-Encoding key is missing in bagit.txt.");

            if (!string.Equals(encoding, "UTF-8", StringComparison.OrdinalIgnoreCase))
                throw new FormatException($"Unsupported Tag-File-Character-Encoding: {encoding}");
        }

        internal void Has_Valid_BaginfoTXT(string path, bool fast)
        {
            
            var bagInfoFile = Path.Combine(path, "bag-info.txt");
            if (!Path.Exists(bagInfoFile) && !fast)
            {
                //just return we already have logged that there is no bag-info
                return;
            }

            if (!Path.Exists(bagInfoFile) && fast)
            {
                var bag = Path.GetDirectoryName(path);
                _logger.LogCritical("{b} does not contain a bag-info.txt file, required for fast validation.", bag );
                throw new InvalidDataException("bag-info.txt is required for fast validation");
            }



            var tags = TagFile.GetTagFileAsDict(bagInfoFile);
            if(!tags.ContainsKey("Payload-Oxum") && fast)
            {
                _logger.LogCritical("bag-info.txt does not contain a Payload-Oxum tag, required for fast validation.");
                throw new InvalidDataException("Payload-Oxum is required for fast validation");
            }


            if (!tags.TryGetValue("Payload-Oxum", out var version))
            {
                _logger.LogWarning("bag-info.txt does not contain a Payload-Oxum, skipping validation");
            }
            else
            {
                var oxum = _bagInfoService.GetOxum(path);
                var payloadOxum = tags["Payload-Oxum"];
                if (oxum != payloadOxum)
                {
                    _logger.LogCritical($"was expecting {payloadOxum} returned {oxum}");
                    throw new InvalidDataException("Payload-Oxum in bag-info.txt does not match.");
                }
            }
        }

        internal void ValidateManifests(string bagPath)
        {
            var validatedManifestCounter = 0;
            foreach (var f in Directory.EnumerateFiles(bagPath))
            {
                
                if (System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(f), @"^(manifest|tagmanifest)-(md5|sha1|sha256|sha384|sha512)\.txt$")) 
                {
                    //_logger.LogInformation($"Validating: {f}");
                    validatedManifestCounter++;
                    _manifestService.ValidateManifestFile(f);
                }
            }

            if(validatedManifestCounter == 0)
            {
                throw new InvalidDataException($"{bagPath} did not contain any manifest files.");
            }
        }
    }
}
