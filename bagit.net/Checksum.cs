using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace bagit.net
{
    public enum ChecksumAlgorithm
    {
        MD5,
        SHA1,
        SHA256,
        SHA384,
        SHA512
    }
    public static class Checksum
    {
        public static string CalculateChecksum(string filePath, ChecksumAlgorithm algorithm)
        {
            ArgumentNullException.ThrowIfNull(filePath);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }


            using var hashAlgorithm = algorithm switch
            {
                ChecksumAlgorithm.MD5 => (HashAlgorithm)MD5.Create(),
                ChecksumAlgorithm.SHA1 => (HashAlgorithm)SHA1.Create(),
                ChecksumAlgorithm.SHA256 => (HashAlgorithm)SHA256.Create(),
                ChecksumAlgorithm.SHA384 => (HashAlgorithm)SHA384.Create(),
                ChecksumAlgorithm.SHA512 => (HashAlgorithm)SHA512.Create(),
                _ => throw new NotSupportedException($"Algorithm {algorithm} not supported.")
            };

            using var fileStream = File.OpenRead(filePath);
            byte[] hashBytes = hashAlgorithm.ComputeHash(fileStream);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        public static bool CompareChecksum(string path, string checksum, ChecksumAlgorithm algorithm)
        {
            var calculatedMD5 = CalculateChecksum(path, algorithm);

            return calculatedMD5.Equals(checksum, StringComparison.OrdinalIgnoreCase);

        }
    }
}
