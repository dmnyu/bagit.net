using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

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
            

            //get the checksum algorithm
            Match match = Regex.Match(fn, Bagit.checksumPattern, RegexOptions.IgnoreCase);
            if (!match.Success)
                throw new InvalidDataException($"Cannot determine checksum algorithm from manifest filename '{fn}'.");

            ChecksumAlgorithm algorithm = Bagit.Algorithms[match.Groups[1].Value.ToLowerInvariant()];

            //validate manifest
            foreach (var line in File.ReadLines(manifestFile))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue; // skip blank lines (allowed by BagIt)

                
                var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    throw new InvalidDataException($"Invalid manifest line format: '{line}'.");

                string checksum = parts[0];
                string payloadFile = parts[1].Trim();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    payloadFile = payloadFile
                        .Replace('/', Path.DirectorySeparatorChar)
                        .Replace('\\', Path.DirectorySeparatorChar);
                }
                string fullPath = Path.Combine(dir, payloadFile);

                // Verify checksum
                Bagit.Logger.LogInformation($"Verifying checksum for file {fullPath}");
                bool result = Checksum.CompareChecksum(fullPath, checksum, algorithm);
                if (!result)
                {
                    throw new InvalidDataException(
                        $"Checksum mismatch for '{payloadFile}' in manifest '{fn}'. Expected: {checksum}"
                    );
                }
            }
        }

    }
}
