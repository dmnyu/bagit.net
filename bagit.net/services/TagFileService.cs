using bagit.net.interfaces;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;

namespace bagit.net.services
{
    public class TagFileService : ITagFileService
    {
        private readonly ILogger _logger;
        private string nl = Environment.NewLine;
        private readonly IFileManagerService _fileManagerService;

        public TagFileService(ILogger<TagFileService> logger, IFileManagerService fileManagerService)
        {
            _logger = logger;
            _fileManagerService = fileManagerService;
        }

        public Dictionary<string, string> GetTagFileAsDict(string tagFilePath)
        {
            var tagDictionary = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(tagFilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(": ", 2, StringSplitOptions.None);

                if (parts.Length != 2)
                    throw new FormatException($"Invalid tag file line: {line}");
                if (tagDictionary.ContainsKey(parts[0]))
                    throw new FormatException($"tag file contains duplicate key {parts[0]}");
                tagDictionary.Add(parts[0], parts[1]);
            }

            return tagDictionary;
        }

        public void CreateBagInfo(string bagDir)
        {
            var oxum = GetOxum(bagDir);
            var sb = new StringBuilder();

            sb.AppendLine($"Bag-Software-Agent: bagit.net v{Bagit.VERSION}");
            sb.AppendLine($"BagIt-Version: {Bagit.BAGIT_VERSION}");
            sb.AppendLine($"Bagging-Date: {DateTime.UtcNow:yyyy-MM-dd}");
            sb.AppendLine($"Payload-Oxum: {oxum}");

            var bagInfoFile = Path.Combine(bagDir, "bag-info.txt");
            File.WriteAllText(bagInfoFile, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        public void CreateBagItTXT(string bagRoot) 
        {
            var bagitTxt = Path.Combine(bagRoot, "bagit.txt");
            if (!System.Text.RegularExpressions.Regex.IsMatch(Bagit.BAGIT_VERSION, @"^\d+\.\d+$"))
            {
                _logger.LogCritical("Invalid BagIt version: {b}. Must be in 'major.minor' format.", Bagit.BAGIT_VERSION);
                throw new InvalidOperationException($"Invalid BagIt version: {Bagit.BAGIT_VERSION}. Must be in 'major.minor' format.");
            }
            var content = $"BagIt-Version: {Bagit.BAGIT_VERSION}{nl}Tag-File-Character-Encoding: UTF-8{nl}";
            File.WriteAllText(bagitTxt, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        public string GetOxum(string bagRoot)
        {
            string dataDir = Path.Combine(bagRoot, "data");
            int count = 0;
            long numBytes = 0;

            if (!Directory.Exists(dataDir))
                throw new ArgumentException($"Data directory does not exist: {dataDir}");

            foreach (var file in Directory.EnumerateFiles(dataDir, "*", SearchOption.AllDirectories))
            {
                var info = new FileInfo(file);
                numBytes += info.Length;
                count++;
            }

            return $"{numBytes}.{count}";
        }

        public List<KeyValuePair<string, string>> GetTagFileAsList(string baginfoPath)
        {
            return File.ReadAllLines(baginfoPath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line =>
                {
                    var parts = line.Split(": ", 2);
                    if (parts.Length != 2)
                        throw new FormatException($"Invalid bag-info.txt line: {line}");
                    return new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim());
                })
                .ToList();
        }

        public void ValidateBagitTXT(string bagRoot)
        {
            var bagitPath = Path.Combine(bagRoot, "bagit.txt");
            if (!File.Exists(bagitPath))
                throw new FileNotFoundException("bagit.txt is missing from the bag root.", bagitPath);

            var tags = GetTagFileAsDict(bagitPath);

            if (!tags.TryGetValue("BagIt-Version", out var version))
                throw new FormatException("BagIt-Version key is missing in bagit.txt.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(version, @"^\d+\.\d+$"))
                throw new FormatException($"Invalid BagIt-Version format: {version}");

            if (!tags.TryGetValue("Tag-File-Character-Encoding", out var encoding))
                throw new FormatException("Tag-File-Character-Encoding key is missing in bagit.txt.");

            if (!string.Equals(encoding, "UTF-8", StringComparison.OrdinalIgnoreCase))
                throw new FormatException($"Unsupported Tag-File-Character-Encoding: {encoding}");
        }

        public bool HasBagInfo(string bagRoot)
        {
            if(!Path.Exists(Path.Combine(bagRoot, "bag-info.txt"))) 
            {
                return false;
            } 
            return true;    
        }

        public bool ValidateBagInfo(string bagRoot)
        {
            //check that bag-info exists
            var _bagInfo = Path.Combine(bagRoot, "bag-info.txt");
            if(!HasBagInfo(bagRoot))
                return false;
            
            //check that it is valid UTF8
            if(!_fileManagerService.IsValidUTF8(_bagInfo))
                return false;

            //check that it does not contain a BOM
            if (_fileManagerService.HasBOM(_bagInfo))
                return false;

            //get the dictionary as tags
            var _tags = GetTags(_bagInfo);

            //ensure there is only one value for the following
            if (_tags["Payload-Oxum"].Count > 1)
                return false;
            if (_tags["Bag-Software-Agent"].Count > 1)
                return false;
            if (_tags["BagIt-Version"].Count > 1)
                return false;
            if (_tags["Bagging-Date"].Count > 1)
                return false;

            return true;
        }

        public Dictionary<string, List<string>> GetTags(string tagFilePath)
        {
            var _tags = new Dictionary<string, List<string>>();

            var lines = File.ReadAllLines(tagFilePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            foreach( var line in lines)
            {
                var parts = line.Split(new[] { ':' }, 2);
                if (parts.Length != 2)
                    throw new FormatException($"Invalid bag-info.txt line: {line}");
     
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                if (_tags.ContainsKey(key))
                {
                    _tags[key].Add(value);
                }
                else
                {
                    _tags[key] = new List<string>() { value };
                }
            }

            return _tags;
        }
    }
}
