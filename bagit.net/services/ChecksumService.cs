using bagit.net.interfaces;
using System.Security.Cryptography;
using System.Text.RegularExpressions;


namespace bagit.net.services
{

    public class ChecksumService : IChecksumService
    {
        public string CalculateChecksum(string? filePath, ChecksumAlgorithm algorithm)
        {
            ArgumentNullException.ThrowIfNull(filePath);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }


            using var hashAlgorithm = algorithm switch
            {
                ChecksumAlgorithm.MD5 => MD5.Create(),
                ChecksumAlgorithm.SHA1 => SHA1.Create(),
                ChecksumAlgorithm.SHA256 => SHA256.Create(),
                ChecksumAlgorithm.SHA384 => SHA384.Create(),
                ChecksumAlgorithm.SHA512 => (HashAlgorithm)SHA512.Create(),
                _ => throw new NotSupportedException($"Algorithm {algorithm} not supported.")
            };

            using var fileStream = File.OpenRead(filePath);
            byte[] hashBytes = hashAlgorithm.ComputeHash(fileStream);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        public bool CompareChecksum(string? path, string checksum, ChecksumAlgorithm algorithm)
        {
            var cleanedChecksum = CleanChecksum(checksum, algorithm);
            if (cleanedChecksum is null)
                return false;

            var calculatedMD5 = CalculateChecksum(path, algorithm);

            return calculatedMD5.Equals(cleanedChecksum, StringComparison.OrdinalIgnoreCase);
        }

        public string GetAlgorithmCode(ChecksumAlgorithm algorithm)
        {
            if (!Enum.IsDefined(typeof(ChecksumAlgorithm), algorithm))
                throw new ArgumentOutOfRangeException(nameof(algorithm), $"Unsupported algorithm: {algorithm}");

            return algorithm switch
            {
                ChecksumAlgorithm.MD5 => "md5",
                ChecksumAlgorithm.SHA1 => "sha1",
                ChecksumAlgorithm.SHA256 => "sha256",
                ChecksumAlgorithm.SHA384 => "sha384",
                ChecksumAlgorithm.SHA512 => "sha512",
                _ => throw new ArgumentOutOfRangeException(nameof(algorithm), $"Unsupported algorithm: {algorithm}")
            };
        }

        private string CleanChecksum(string checksum, ChecksumAlgorithm algorithm)
        {
            if (string.IsNullOrWhiteSpace(checksum))
                throw new ArgumentNullException("the checksum passed is blank or null");


            checksum = checksum.Trim();

            checksum = checksum.ToLowerInvariant();

            checksum = Regex.Replace(checksum, @"[^0-9A-Fa-f]", "");
            int expectedLen = algorithm switch
            {
                ChecksumAlgorithm.MD5 => 32,
                ChecksumAlgorithm.SHA1 => 40,
                ChecksumAlgorithm.SHA256 => 64,
                ChecksumAlgorithm.SHA384 => 96,
                ChecksumAlgorithm.SHA512 => 128,
                _ => 0
            };

            if (expectedLen > 0 && checksum.Length != expectedLen)
                throw new ArgumentException($"the specified algorithm size for {algorithm} and the checksum size do not match.");

            return checksum;
        }
    }

}