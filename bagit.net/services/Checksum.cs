using System.Security.Cryptography;
using System.Text.RegularExpressions;


namespace bagit.net.services
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
        public static Dictionary<string, ChecksumAlgorithm> Algorithms = new Dictionary<string, ChecksumAlgorithm>()
        {
            {"md5", ChecksumAlgorithm.MD5},
            {"sha1", ChecksumAlgorithm.SHA1},
            {"sha256", ChecksumAlgorithm.SHA256},
            {"sha384", ChecksumAlgorithm.SHA384},
            {"sha512", ChecksumAlgorithm.SHA512}
        };

        public const string checksumPattern = @"-(md5|sha1|sha256|sha384|sha512)\b";
        public const string ManifestPattern = @"manifest-(md5|sha1|sha256|sha384|sha512).txt";
        public const string TagmanifestPattern = @"tagmanifest-(md5|sha1|sha256|sha384|sha512).txt";
        public static string CalculateChecksum(string? filePath, ChecksumAlgorithm algorithm)
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

        public static bool CompareChecksum(string? path, string checksum, ChecksumAlgorithm algorithm)
        {
            var cleanedChecksum = CleanChecksum(checksum, algorithm);
            if (cleanedChecksum is null)
                return false;

            var calculatedMD5 = CalculateChecksum(path, algorithm);

            return calculatedMD5.Equals(cleanedChecksum, StringComparison.OrdinalIgnoreCase);
        }

        private static string CleanChecksum(string checksum, ChecksumAlgorithm algorithm)
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

        internal static string GetAlgorithmCode(ChecksumAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case ChecksumAlgorithm.MD5: return "md5";
                case ChecksumAlgorithm.SHA1: return "sha1";
                case ChecksumAlgorithm.SHA256: return "sha256";
                case ChecksumAlgorithm.SHA384: return "sha384";
                case ChecksumAlgorithm.SHA512: return "sha512";
                default: throw new ArgumentException($"checksum algorithm: {algorithm} is not supported");
            }
        }
    }

}