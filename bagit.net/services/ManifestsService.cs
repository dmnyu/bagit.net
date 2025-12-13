using bagit.net.domain;
using bagit.net.interfaces;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace bagit.net.services
{
    public class ManifestService : IManifestService
    {

        private readonly IChecksumService _checksumService;
        private readonly IMessageService _messageService;
        private readonly IFileManagerService _fileManagerService;
        private static readonly Regex _manifestRegex = new(@"^(manifest|tagmanifest)-(md5|sha1|sha256|sha384|sha512)\.txt$", RegexOptions.Compiled);
        private static readonly Regex _nonCREndingsRegex = new(@"\r(?!\n)");
        public static readonly Regex _checkSumRegex = new(@"-(md5|sha1|sha256|sha384|sha512)\b", RegexOptions.Compiled);
        public static readonly Regex _tagmanifestRegex = new(@"tagmanifest-(md5|sha1|sha256|sha384|sha512).txt$", RegexOptions.Compiled);


        public ManifestService(IChecksumService checksumService, IMessageService messageService, IFileManagerService fileManagerService)
        {
            _checksumService = checksumService;
            _fileManagerService = fileManagerService;
            _messageService = messageService;
        }


    public async Task CreatePayloadManifest(string bagRoot, IEnumerable<ChecksumAlgorithm> algorithms, int processes)
    {
        
        _messageService.Add(new MessageRecord(MessageLevel.INFO, $"Using {processes} processes to generate manifests: {string.Join(",", algorithms)}"));
        // Prepare per-algorithm StringBuilders and lock objects
        var checksumManifests = algorithms
            .Distinct()
            .ToDictionary(
                alg => _checksumService.GetAlgorithmCode(alg),
                alg => new StringBuilder()
            );

        var lockObjects = algorithms.ToDictionary(
            alg => _checksumService.GetAlgorithmCode(alg),
            alg => new object()
        );

        var semaphore = new SemaphoreSlim(processes);
        var fileEntries = GetPayloadFiles(bagRoot);

        // Start a task for each file
        var tasks = fileEntries.Select(async entry =>
        {
            var currentEntry = entry; // local copy to avoid async capture issues
            await semaphore.WaitAsync();
            try
            {
                _messageService.Add(new MessageRecord(MessageLevel.INFO, $"Generating manifest lines for file {currentEntry}"));

                foreach (var algorithm in algorithms)
                {
                    var algorithmCode = _checksumService.GetAlgorithmCode(algorithm);
                    var checksum = await _checksumService.CalculateChecksum(Path.Combine(bagRoot, currentEntry), algorithm);

                    // Append checksum line safely
                    lock (lockObjects[algorithmCode])
                    {
                        checksumManifests[algorithmCode].AppendLine($"{checksum} {currentEntry}");
                    }
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        // Wait for all files to finish
        await Task.WhenAll(tasks);

        // Write out each manifest
        foreach (var kvp in checksumManifests)
        {
            var manifestPath = Path.Combine(bagRoot, $"manifest-{kvp.Key}.txt");
            await File.WriteAllTextAsync(manifestPath, kvp.Value.ToString());
        }
    }

    public void CreateTagManifestFile(string bagRoot, IEnumerable<ChecksumAlgorithm> algorithms)
        {

            var checksumManifests = algorithms
                .Distinct()
                .ToDictionary(
                    alg => _checksumService.GetAlgorithmCode(alg),
                    alg => new StringBuilder()
            );

            var fileEntries = GetRootFiles(bagRoot);

            foreach (var entry in fileEntries)
            {
                foreach (var algorithm in algorithms)
                {
                    var algorithmCode = _checksumService.GetAlgorithmCode(algorithm);
                    var checksum = _checksumService.CalculateChecksum(Path.Combine(bagRoot, entry), algorithm).GetAwaiter().GetResult();
                    checksumManifests[algorithmCode].AppendLine($"{checksum} {entry}");
                }
            }

            foreach (var algorithm in algorithms) {
                var algorithmCode = _checksumService.GetAlgorithmCode(algorithm);
                var manifestFilename = Path.Combine(bagRoot, $"tagmanifest-{algorithmCode}.txt");
                _fileManagerService.WriteToFile(manifestFilename, checksumManifests[algorithmCode].ToString());
            }
        }

        public void UpdateTagManifest(string bagRoot)
        {
            var rootFiles = Directory.GetFiles(bagRoot);
            foreach (var rootFile in rootFiles)
            {
                if (_tagmanifestRegex.IsMatch(rootFile))
                {
                    _messageService.Add(new MessageRecord(MessageLevel.INFO, $"Updating {rootFile}"));
                    var algorithm = GetManifestAlgorithm(rootFile);
                    var tmpFile = _fileManagerService.CreateTempFile(bagRoot);
                    StringBuilder sb = new StringBuilder();
                    var fileEntries = GetRootFiles(bagRoot);
                    foreach (var entry in fileEntries)
                    {
                        if (Path.GetFileName(entry) != Path.GetFileName(tmpFile) && Path.GetFileName(entry) != Path.GetFileName(rootFile))
                        {
                            var checksum = _checksumService.CalculateChecksum(Path.Combine(bagRoot, entry), algorithm);
                            sb.Append($"{checksum} {entry}\n");
                        }
                    }
                    _fileManagerService.WriteToFile(tmpFile, sb.ToString());
                    _fileManagerService.DeleteFile(rootFile);
                    _fileManagerService.MoveFile(tmpFile, rootFile);
                }
            }

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

        internal ChecksumAlgorithm GetManifestAlgorithm(string manifestFilename)
        {
            Match match = _checkSumRegex.Match(manifestFilename);
            if (!match.Success)
                throw new InvalidDataException($"Cannot determine checksum algorithm from manifest filename '{manifestFilename}'.");

            return ChecksumAlgorithmMap.Algorithms[match.Groups[1].Value.ToLowerInvariant()];
        }

        /* --------------------
         * 
         * Validation Methods
         * 
         ----------------------*/ 

        public async Task ValidateManifestFiles(string bagRoot, int processes)
        {
            _messageService.Add(new MessageRecord(MessageLevel.INFO, $"validating checksums using {processes} processes"));
            var manifestEntries = ParseManifests(bagRoot);

            using var semaphore = new SemaphoreSlim(processes);

            var tasks = manifestEntries.Select(async manifestEntry =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await _checksumService.CompareChecksums(manifestEntry.Key, manifestEntry.Value);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
            
        }

        public Dictionary<string, Dictionary<ChecksumAlgorithm, string>> ParseManifests(string bagRoot)
        {
            var payloadExpectations = new Dictionary<string, Dictionary<ChecksumAlgorithm, string>>();
            var manifestFiles = Directory
                .EnumerateFiles(bagRoot)
                .Where(f => _manifestRegex.IsMatch(Path.GetFileName(f)))
                .ToList();

            foreach (var manifestFile in manifestFiles) {
                var algorithm = GetManifestAlgorithm(manifestFile);
                var lines = File.ReadAllLines(manifestFile);
                foreach (var line in lines) {
                    var (payloadfile, checksumValue) = ValidateManifestLine(line);
                    var payloadPath = Path.Combine(bagRoot, payloadfile);
                    if (payloadExpectations.ContainsKey(payloadPath))
                        payloadExpectations[payloadPath].Add(algorithm, checksumValue);
                    else
                    {
                        payloadExpectations[payloadPath] = [];
                        payloadExpectations[payloadPath].Add(algorithm, checksumValue);
                    }

                }
            }
            return payloadExpectations;
        }
        
     

        public (string payloadFile, string hash) ValidateManifestLine(string line)
        {
            var parts = line.Split(' ', 2, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"Invalid manifest line format: '{line}'."));
                return (string.Empty, string.Empty);
            }

            string checksum = parts[0].Trim();
            string payloadFile = parts[1].Trim();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                payloadFile = payloadFile
                    .Replace('/', Path.DirectorySeparatorChar)
                    .Replace('\\', Path.DirectorySeparatorChar);
            }

            if (line.Length > 200)
                _messageService.Add(new MessageRecord(MessageLevel.WARNING, $"Manifest line exceeds 200 characters, may be too long for some file systems: {line}"));

            if (!IsValidUtf8(payloadFile))
                _messageService.Add(new MessageRecord(MessageLevel.WARNING, $"{payloadFile} contains non-unicode characters"));

            if (!payloadFile.IsNormalized(NormalizationForm.FormC))
                _messageService.Add(new MessageRecord(MessageLevel.WARNING, $"{payloadFile} is not NFC-Normalized"));

            return(payloadFile, checksum);

        }

        public void ValidateManifestLineEndings(string manifestFile)
        {
            string content = File.ReadAllText(manifestFile, Encoding.UTF8);

            // Match all line endings (either LF or CRLF)
            var lineEndingPattern = @"\r?\n";
            var matches = Regex.Matches(content, lineEndingPattern);

            if (matches.Count == 0)
              _messageService.Add(new MessageRecord(MessageLevel.WARNING, $"{Path.GetFileName(manifestFile)} contains no line endings"));

            // Check for any CR-only line endings (\r not followed by \n)
            if (_nonCREndingsRegex.IsMatch(content))
            {
                _messageService.Add(new MessageRecord(MessageLevel.WARNING, $"{Path.GetFileName(manifestFile)} contains CR-only line endings"));
            }

            // Check if last line ends with a newline
            if (!content.EndsWith("\n") && !content.EndsWith("\r"))
            {
                _messageService.Add(new MessageRecord(MessageLevel.WARNING, ($"{Path.GetFileName(manifestFile)} does not end with a newline")));
            }
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

        public void ValidateManifestFilesCompleteness(string bagRoot)
        {
            
            var validatedManifestCounter = 0;
            foreach (var manifestFile in Directory.EnumerateFiles(bagRoot))
            {

                if (_manifestRegex.IsMatch(Path.GetFileName(manifestFile)))
                {
                    validatedManifestCounter++;
                    ValidateManifestFileCompleteness(manifestFile);  
                }
            }

            if (validatedManifestCounter == 0)
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"{bagRoot} did not contain any manifest files."));
            }
        }

        public void ValidateManifestFileCompleteness(string manifestFile)
        { 
            var bagRoot = Path.GetDirectoryName(manifestFile);
            List<KeyValuePair<string, string>> payloadFiles = GetManifestAsKeyValuePairs(manifestFile);
            foreach (var payloadFile in payloadFiles) {
                var payloadFilePath = Path.GetFullPath(Path.Combine(bagRoot!, payloadFile.Value));
                if (!File.Exists(payloadFilePath)) {
                    _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"{payloadFilePath} does not exist"));
                }
            } 
        }
    }
}
