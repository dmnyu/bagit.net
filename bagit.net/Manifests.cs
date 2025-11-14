using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Enumeration;

namespace bagit.net
{
    public static class Manifest
    {
        public static void CreatePayloadManifest(string bagRoot, ChecksumAlgorithm algorithm)
        {
            var manifestContent = new StringBuilder();
            var fileEntries = GetPayloadFiles(bagRoot);
            foreach (var entry in fileEntries)
            {
                var checksum = Checksum.CalculateChecksum(Path.Combine(bagRoot, entry), algorithm);
                manifestContent.AppendLine($"{checksum} {entry}");
            }

            var algorithmCode = Checksum.GetAlgorithmCode(algorithm);
            var manifestFilename = Path.Combine(bagRoot, $"manifest-{algorithmCode}.txt");
            File.WriteAllText(manifestFilename, manifestContent.ToString(), Encoding.UTF8);
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
    }
}
