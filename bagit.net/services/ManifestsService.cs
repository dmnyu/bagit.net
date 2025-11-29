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
                manifestContent.AppendLine($"{checksum} {entry}");
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
                sb.AppendLine($"{checksum} {entry}");
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

        public void ValidateManifestFiles(string bagPath)
        {
            var validatedManifestCounter = 0;
            foreach (var f in Directory.EnumerateFiles(bagPath))
            {

                if (System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(f), @"^(manifest|tagmanifest)-(md5|sha1|sha256|sha384|sha512)\.txt$"))
                {
                    validatedManifestCounter++;
                    ValidateManifestFile(f);
                }
            }

            if (validatedManifestCounter == 0)
            {
                throw new InvalidDataException($"{bagPath} did not contain any manifest files.");
            }
        }

        public void ValidateManifestFile(string manifestFile)
        {
            string dir = Path.GetDirectoryName(manifestFile)
                ?? throw new InvalidDataException("Could not determine manifest directory.");

            string fn = Path.GetFileName(manifestFile);

            ChecksumAlgorithm algorithm = GetManifestAlgorithm(fn);

            ValidateManifestLineEndings(manifestFile);

            
            foreach (var line in File.ReadLines(manifestFile))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                ValidateManifestLine(line, dir, fn, algorithm);
            }
        }

        internal ChecksumAlgorithm GetManifestAlgorithm(string manifestFilename)
        {
            Match match = Regex.Match(manifestFilename, ServiceHelpers.ChecksumPattern, RegexOptions.IgnoreCase);
            if (!match.Success)
                throw new InvalidDataException($"Cannot determine checksum algorithm from manifest filename '{manifestFilename}'.");

            return ChecksumAlgorithmMap.Algorithms[match.Groups[1].Value.ToLowerInvariant()];
        }

        internal void ValidateManifestLine(string line, string manifestDir, string manifestFileName, ChecksumAlgorithm algorithm)
        {
            var parts = line.Split(' ', 2, StringSplitOptions.None);
            if (parts.Length != 2)
                throw new InvalidDataException($"Invalid manifest line format: '{line}'.");

            string checksum = parts[0];
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

           
            ValidateLineLength(line);
            ValidateFilenameUtf8(payloadFile);
            ValidateFilenameNormalization(filename, payloadFile);

            _logger.LogInformation($"Verifying checksum for file {fullPath}");
            if (!_checksumService.CompareChecksum(fullPath, checksum, algorithm))
            {
                throw new InvalidDataException(
                    $"Checksum mismatch for '{payloadFile}' in manifest '{manifestFileName}'. Expected: {checksum}"
                );
            }
        }

        internal void ValidateLineLength(string line, int maxLength = 200)
        {
            if (line.Length > maxLength)
                _logger.LogWarning($"Manifest line exceeds {maxLength} characters, may be too long for some file systems: {line}");
        }

        internal void ValidateFilenameUtf8(string filename)
        {
            if (!IsValidUtf8(filename))
                _logger.LogWarning($"{filename} contains non-unicode characters");
        }

        internal void ValidateFilenameNormalization(string filename, string fullPath)
        {
            if (!filename.IsNormalized(NormalizationForm.FormC))
                _logger.LogWarning($"{fullPath} is not NFC-Normalized");
        }


        internal void ValidateManifestLineEndings(string manifestFile)
        {
            string content = File.ReadAllText(manifestFile, Encoding.UTF8);

            // Match all line endings (either LF or CRLF)
            var lineEndingPattern = @"\r?\n";
            var matches = Regex.Matches(content, lineEndingPattern);

            if (matches.Count == 0)
            {
                _logger.LogWarning($"{Path.GetFileName(manifestFile)} contains no line endings");
                return;
            }

            // Check for any CR-only line endings (\r not followed by \n)
            if (Regex.IsMatch(content, @"\r(?!\n)"))
            {
                _logger.LogWarning($"{Path.GetFileName(manifestFile)} contains CR-only line endings");
            }

            // Check if last line ends with a newline
            if (!content.EndsWith("\n") && !content.EndsWith("\r"))
            {
                _logger.LogWarning($"{Path.GetFileName(manifestFile)} does not end with a newline");
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

    }
}
