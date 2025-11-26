using bagit.net.interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace bagit.net.services
{
    public class TagFileService : ITagFileService
    {
        private readonly ILogger _logger;
        private string nl = Environment.NewLine;
        public TagFileService(ILogger<TagFileService> logger)
        {
            _logger = logger;
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
            _logger.LogInformation("Creating bag-info.txt");
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
            _logger.LogInformation("Creating bagit.txt");
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

    }
}
