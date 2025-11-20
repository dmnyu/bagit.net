using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace bagit.net
{
    public static class Manifest
    {
        internal static void CreatePayloadManifest(string bagRoot, ChecksumAlgorithm algorithm)
        {
            var algorithmCode = Checksum.GetAlgorithmCode(algorithm);
            Bagit.Logger.LogInformation($"Generating manifests using {algorithmCode}");
            var manifestContent = new StringBuilder();
            var fileEntries = GetPayloadFiles(bagRoot);
            foreach (var entry in fileEntries)
            {
                Bagit.Logger.LogInformation($"Generating manifest lines for file {entry}");
                var checksum = Checksum.CalculateChecksum(Path.Combine(bagRoot, entry), algorithm);
                manifestContent.AppendLine($"{checksum} {entry}");
            }

            var manifestFilename = Path.Combine(bagRoot, $"manifest-{algorithmCode}.txt");
            File.WriteAllText(manifestFilename, manifestContent.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        internal static void CreateTagManifestFile(string bagRoot, ChecksumAlgorithm algorithm)
        {
            var algorithmCode = Checksum.GetAlgorithmCode(algorithm);
            var manifestFilename = Path.Combine(bagRoot, $"tagmanifest-{algorithmCode}.txt");
            Bagit.Logger.LogInformation($"Creating {manifestFilename}");
            
            StringBuilder sb  = new StringBuilder();
            var fileEntries = GetRootFiles(bagRoot);
            foreach (var entry in fileEntries)
            {
                var checksum = Checksum.CalculateChecksum(Path.Combine(bagRoot, entry), algorithm);
                sb.AppendLine($"{checksum} {entry}");
            }
            
            File.WriteAllText(manifestFilename, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        internal static IEnumerable<string> GetPayloadFiles(string bagRoot)
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

        internal static IEnumerable<string> GetRootFiles(string bagRoot)
        {
            foreach (var file in Directory.EnumerateFiles(bagRoot, "*", SearchOption.TopDirectoryOnly))
            {
                string relativePath = Path.GetRelativePath(bagRoot, file);
                relativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
                yield return relativePath;
            }
        }

        public static List<KeyValuePair<string, string>> GetManifestAsKeyValuePairs(string manifestPath)
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

        internal static void ValidateManifestFile(string manifestFile)
        {
            string dir = Path.GetDirectoryName(manifestFile)
                ?? throw new InvalidDataException("Could not determine manifest directory.");

            string fn = Path.GetFileName(manifestFile);

            // Determine checksum algorithm
            ChecksumAlgorithm algorithm = GetManifestAlgorithm(fn);

            // Check line endings
            ValidateManifestLineEndings(manifestFile);

            // Validate each line
            foreach (var line in File.ReadLines(manifestFile))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                ValidateManifestLine(line, dir, fn, algorithm);
            }
        }

        // Extract checksum algorithm from filename
        private static ChecksumAlgorithm GetManifestAlgorithm(string manifestFilename)
        {
            Match match = Regex.Match(manifestFilename, Bagit.checksumPattern, RegexOptions.IgnoreCase);
            if (!match.Success)
                throw new InvalidDataException($"Cannot determine checksum algorithm from manifest filename '{manifestFilename}'.");

            return Bagit.Algorithms[match.Groups[1].Value.ToLowerInvariant()];
        }

        // Validate a single line
        private static void ValidateManifestLine(string line, string manifestDir, string manifestFileName, ChecksumAlgorithm algorithm)
        {
            var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
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

            // Checks
            ValidateLineLength(line);
            ValidateFilenameUtf8(payloadFile);
            ValidateFilenameNormalization(filename, payloadFile);

            // Verify checksum
            Bagit.Logger.LogInformation($"Verifying checksum for file {fullPath}");
            if (!Checksum.CompareChecksum(fullPath, checksum, algorithm))
            {
                throw new InvalidDataException(
                    $"Checksum mismatch for '{payloadFile}' in manifest '{manifestFileName}'. Expected: {checksum}"
                );
            }
        }

        // Line length check
        private static void ValidateLineLength(string line, int maxLength = 200)
        {
            if (line.Length > maxLength)
                Bagit.Logger.LogWarning($"Manifest line exceeds {maxLength} characters, may be too long for some file systems: {line}");
        }

        // UTF-8 check
        private static void ValidateFilenameUtf8(string filename)
        {
            if (!IsValidUtf8(filename))
                Bagit.Logger.LogWarning($"{filename} contains non-unicode characters");
        }

        // NFC normalization check
        private static void ValidateFilenameNormalization(string filename, string fullPath)
        {
            if (!filename.IsNormalized(NormalizationForm.FormC))
                Bagit.Logger.LogWarning($"{fullPath} is not NFC-Normalized");
        }


        internal static void ValidateManifestLineEndings(string manifestFile)
        {
            string content = File.ReadAllText(manifestFile, System.Text.Encoding.UTF8);

            // Match all line endings (either LF or CRLF)
            var lineEndingPattern = @"\r?\n";
            var matches = Regex.Matches(content, lineEndingPattern);

            if (matches.Count == 0)
            {
                Bagit.Logger.LogWarning($"{Path.GetFileName(manifestFile)} contains no line endings");
                return;
            }

            // Check for any CR-only line endings (\r not followed by \n)
            if (Regex.IsMatch(content, @"\r(?!\n)"))
            {
                Bagit.Logger.LogWarning($"{Path.GetFileName(manifestFile)} contains CR-only line endings");
            }

            // Check if last line ends with a newline
            if (!content.EndsWith("\n") && !content.EndsWith("\r"))
            {
                Bagit.Logger.LogWarning($"{Path.GetFileName(manifestFile)} does not end with a newline");
            }
        }


        internal static bool IsValidUtf8(string filename)
        {
            try
            {
                // Encode string to UTF-8 bytes
                byte[] bytes = Encoding.UTF8.GetBytes(filename);

                // Decode back to string
                string decoded = Encoding.UTF8.GetString(bytes);

                // Compare
                return filename == decoded;
            }
            catch
            {
                return false; // invalid Unicode
            }
        }

    }
}
