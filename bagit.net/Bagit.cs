using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Serilog;
using Serilog.Events;
using Serilog.Formatting;

namespace bagit.net
{
    public static class Bagit
    {
        public const string VERSION = "0.2.0-alpha.1"; 
        public const string BAGIT_VERSION = "1.0";

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

    }
}
