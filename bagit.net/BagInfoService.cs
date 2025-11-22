using Microsoft.Extensions.Logging;
using System.Text;

namespace bagit.net
{
    public interface IBagInfoService
    {
        void CreateBagInfo(string bagDir);
        string GetOxum(string bagRoot);

        List<KeyValuePair<string, string>> GetBagInfoAsKeyValuePairs(string baginfoPath);

    }
    public class BagInfoService : IBagInfoService
    {
        
        private readonly ILogger _logger;

        public BagInfoService(ILogger<BagInfoService> logger)
        {
            _logger = logger;
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

        public List<KeyValuePair<string, string>> GetBagInfoAsKeyValuePairs(string baginfoPath)
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
