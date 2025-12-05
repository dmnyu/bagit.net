using bagit.net.domain;
using bagit.net.interfaces;
using System.Security.Cryptography;
using System.Text.RegularExpressions;


namespace bagit.net.services
{

    public class ChecksumService : IChecksumService
    {
        private readonly IMessageService _messageService;
        public ChecksumService(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public string CalculateChecksum(string? filePath, ChecksumAlgorithm algorithm)
        {
            ArgumentNullException.ThrowIfNull(filePath);
            if (!File.Exists(filePath))
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"{filePath} not found."));
                return string.Empty;
            }

            HashAlgorithm hashAlgorithm = algorithm switch
            {
                ChecksumAlgorithm.MD5 => MD5.Create(),
                ChecksumAlgorithm.SHA1 => SHA1.Create(),
                ChecksumAlgorithm.SHA256 => SHA256.Create(),
                ChecksumAlgorithm.SHA384 => SHA384.Create(),
                ChecksumAlgorithm.SHA512 => SHA512.Create(),
                _ => throw new ArgumentOutOfRangeException(nameof(algorithm))
            };

            using (hashAlgorithm)
            using (var fileStream = File.OpenRead(filePath))
            {
                byte[] hashBytes = hashAlgorithm.ComputeHash(fileStream);
                return Convert.ToHexString(hashBytes).ToLowerInvariant();
            }
        }

        public bool CompareChecksum(string? path, string checksum, ChecksumAlgorithm algorithm)
        {
            bool isValid;
            string? cleanedChecksum;    
            (isValid, cleanedChecksum) = CleanChecksum(checksum, algorithm);
            if(!isValid)
                return false;

            var calculatedMD5 = CalculateChecksum(path, algorithm);

            return calculatedMD5.Equals(cleanedChecksum, StringComparison.OrdinalIgnoreCase);
        }

        public string GetAlgorithmCode(ChecksumAlgorithm algorithm)
        {
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

        private (bool, string?) CleanChecksum(string checksum, ChecksumAlgorithm algorithm)
        {
            if (string.IsNullOrWhiteSpace(checksum)) {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, "the checksum passed is blank or null"));
                return (false, null);
            }

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
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"the specified algorithm size for {algorithm} and the checksum size do not match."));
                return (false, null);
            }

            return (true, checksum);
        }
    }

}