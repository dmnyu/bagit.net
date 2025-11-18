using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace bagit.net
{
    public static class Manifest
    {
        public static void CreatePayloadManifest(string bagRoot, ChecksumAlgorithm algorithm)
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

        public static void CreateTagManifestFile(string bagRoot, ChecksumAlgorithm algorithm)
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
    }
}
