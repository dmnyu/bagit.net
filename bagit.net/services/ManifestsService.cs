using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using bagit.net.interfaces;
using bagit.net.domain;

namespace bagit.net.services
{
    public class ManifestService : IManifestService
    {

        private readonly ILogger<ManifestService> _logger;
        private readonly IChecksumService _checksumService;
        private static readonly Regex _manifestRegex = new(@"^(manifest|tagmanifest)-(md5|sha1|sha256|sha384|sha512)\.txt$", RegexOptions.Compiled);
        private static readonly Regex _nonCREndingsRegex = new(@"\r(?!\n)");

        public ManifestService(ILogger<ManifestService> logger, IChecksumService checksumService)
        {
            _checksumService = checksumService;
            _logger = logger;
        }

        public void CreatePayloadManifest(string bagRoot, ChecksumAlgorithm algorithm)
        {
            var algorithmCode = _checksumService.GetAlgorithmCode(algorithm);
            _logger.LogInformation($"Generating manifests using {algorithmCode}");
            var manifestContent = new StringBuilder();
            var fileEntries = GetPayloadFiles(bagRoot);
            foreach (var entry in fileEntries)
            {
                _logger.LogInformation($"Generating manifest lines for file {entry}");
                var checksum = _checksumService.CalculateChecksum(Path.Combine(bagRoot, entry), algorithm);
                manifestContent.Append($"{checksum.Trim()} {entry.Trim()}\n");
            }

            var manifestFilename = Path.Combine(bagRoot, $"manifest-{algorithmCode}.txt");
            File.WriteAllText(manifestFilename, manifestContent.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        public void CreateTagManifestFile(string bagRoot, ChecksumAlgorithm algorithm)
        {
            var algorithmCode = _checksumService.GetAlgorithmCode(algorithm);
            var manifestFilename = Path.Combine(bagRoot, $"tagmanifest-{algorithmCode}.txt");
            _logger.LogInformation($"Creating {manifestFilename}");
            
            StringBuilder sb  = new StringBuilder();
            var fileEntries = GetRootFiles(bagRoot);
            foreach (var entry in fileEntries)
            {
                var checksum = _checksumService.CalculateChecksum(Path.Combine(bagRoot, entry), algorithm);
                sb.Append($"{checksum.Trim()} {entry.Trim()}\n");
            }
            
            File.WriteAllText(manifestFilename, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        internal IEnumerable<string> GetPayloadFiles(string bagRoot)
        {
            string dataDir = Path.Combine(bagRoot, "data");

            if (!Directory.Exists(dataDir))
                yield break;

            foreach (var file in Directory.EnumerateFiles(dataDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(bagRoot, file);
                relativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
                yield return relativePath;
            }
        }

        internal IEnumerable<string> GetRootFiles(string bagRoot)
        {
            foreach (var file in Directory.EnumerateFiles(bagRoot, "*", SearchOption.TopDirectoryOnly))
            {
                string relativePath = Path.GetRelativePath(bagRoot, file);
                relativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
                yield return relativePath;
            }
        }

        public List<KeyValuePair<string, string>> GetManifestAsKeyValuePairs(string manifestPath)
        {
            return File.ReadAllLines(manifestPath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line =>
                {
                    var parts = line.Split(' ', 2);
                    if (parts.Length != 2)
                        throw new FormatException($"Invalid manifest line: {line}");
                    return new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim());
                })
                .ToList();
        }

        public IEnumerable<MessageRecord> ValidateManifestFiles(string bagPath)
        {

            int manifestCounter = 0;
           
            var messages = new List<MessageRecord>();
            foreach (var f in Directory.EnumerateFiles(bagPath))
            {

                if (_manifestRegex.IsMatch(Path.GetFileName(f)))
                {
                    manifestCounter++;
                    messages.AddRange(ValidateManifestFile(f));
                }
            }

            if (manifestCounter == 0)
            {
                messages.Add(new MessageRecord(MessageLevel.ERROR, $"{bagPath} did not contain any manifest files."));
            }

            return messages;
        }

        public IEnumerable<MessageRecord> ValidateManifestFile(string manifestFile)
        {
            var messages = new List<MessageRecord>();
            string bagRoot = Path.GetDirectoryName(manifestFile)
                ?? throw new InvalidDataException("Could not determine manifest directory.");
            string fn = Path.GetFileName(manifestFile);

            ChecksumAlgorithm algorithm = GetManifestAlgorithm(fn);

            messages.AddRange(ValidateManifestLineEndings(manifestFile));

            
            foreach (var line in File.ReadLines(manifestFile))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                messages.AddRange(ValidateManifestLine(line, bagRoot, fn, algorithm));
            }

            return messages;
        }

        internal ChecksumAlgorithm GetManifestAlgorithm(string manifestFilename)
        {
            Match match = Regex.Match(manifestFilename, ServiceHelpers.ChecksumPattern, RegexOptions.IgnoreCase);
            if (!match.Success)
                throw new InvalidDataException($"Cannot determine checksum algorithm from manifest filename '{manifestFilename}'.");

            return ChecksumAlgorithmMap.Algorithms[match.Groups[1].Value.ToLowerInvariant()];
        }

        internal IEnumerable<MessageRecord> ValidateManifestLine(string line, string manifestDir, string manifestFileName, ChecksumAlgorithm algorithm)
        {
            var messages = new List<MessageRecord>();

            var parts = line.Split(' ', 2, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                return new List<MessageRecord>() { new MessageRecord(MessageLevel.ERROR, ($"Invalid manifest line format: '{line}'.")) };
            }

            string checksum = parts[0].Trim();
            string payloadFile = parts[1].Trim();

            // Normalize path for Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                payloadFile = payloadFile
                    .Replace('/', Path.DirectorySeparatorChar)
                    .Replace('\\', Path.DirectorySeparatorChar);
            }

            string fullPath = Path.Combine(manifestDir, payloadFile);
            string filename = Path.GetFileName(payloadFile);


            if (line.Length > 200)
                messages.Add(new MessageRecord(MessageLevel.WARNING, $"Manifest line exceeds 200 characters, may be too long for some file systems: {line}"));

            if (!IsValidUtf8(filename))
                messages.Add(new MessageRecord(MessageLevel.WARNING, $"{filename} contains non-unicode characters"));

            if (!filename.IsNormalized(NormalizationForm.FormC))
                messages.Add(new MessageRecord(MessageLevel.WARNING, $"{fullPath} is not NFC-Normalized"));

            messages.Add(new MessageRecord(MessageLevel.INFO, $"Verifying checksum for file {fullPath}"));
            if (!_checksumService.CompareChecksum(fullPath, checksum, algorithm))
                messages.Add(new MessageRecord(MessageLevel.ERROR, $"Checksum mismatch for '{payloadFile}' in manifest '{manifestFileName}'. Expected: {checksum}"));

            return messages;
        }


        internal IEnumerable<MessageRecord> ValidateManifestLineEndings(string manifestFile)
        {
            var messages = new List<MessageRecord>();
            string content = File.ReadAllText(manifestFile, Encoding.UTF8);

            // Match all line endings (either LF or CRLF)
            var lineEndingPattern = @"\r?\n";
            var matches = Regex.Matches(content, lineEndingPattern);

            if (matches.Count == 0)
              messages.Add(new MessageRecord(MessageLevel.ERROR, $"{Path.GetFileName(manifestFile)} contains no line endings"));

            // Check for any CR-only line endings (\r not followed by \n)
            if (_nonCREndingsRegex.IsMatch(content))
            {
                messages.Add(new MessageRecord(MessageLevel.WARNING, $"{Path.GetFileName(manifestFile)} contains CR-only line endings"));
            }

            // Check if last line ends with a newline
            if (!content.EndsWith("\n") && !content.EndsWith("\r"))
            {
                messages.Add(new MessageRecord(MessageLevel.WARNING, ($"{Path.GetFileName(manifestFile)} does not end with a newline")));
            }

            return messages;
        }


        internal bool IsValidUtf8(string filename)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(filename);
                string decoded = Encoding.UTF8.GetString(bytes);
                return filename == decoded;
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<MessageRecord> ValidateManifestFilesCompleteness(string bagRoot)
        {
            List<MessageRecord> messages = new List<MessageRecord>();
            var validatedManifestCounter = 0;
            foreach (var manifestFile in Directory.EnumerateFiles(bagRoot))
            {

                if (_manifestRegex.IsMatch(Path.GetFileName(manifestFile)))
                {
                    validatedManifestCounter++;
                    messages.AddRange(ValidateManifestFileCompleteness(manifestFile));  
                }
            }

            if (validatedManifestCounter == 0)
            {
                messages.Add(new MessageRecord(MessageLevel.ERROR, $"{bagRoot} did not contain any manifest files."));
            }
            return messages;
        }

        public IEnumerable<MessageRecord> ValidateManifestFileCompleteness(string manifestFile)
        {
            var messages = new List<MessageRecord>();   
            var bagRoot = Path.GetDirectoryName(manifestFile);
            List<KeyValuePair<string, string>> payloadFiles = GetManifestAsKeyValuePairs(manifestFile);
            foreach (var payloadFile in payloadFiles) {
                var payloadFilePath = Path.GetFullPath(Path.Combine(bagRoot!, payloadFile.Value));
                if (!File.Exists(payloadFilePath)) {
                    messages.Add(new MessageRecord(MessageLevel.ERROR, $"{payloadFilePath} does not exist"));
                }
            }
            return messages;
            
        }
    }
}
