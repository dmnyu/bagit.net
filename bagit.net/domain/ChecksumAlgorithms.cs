namespace bagit.net.domain
{


    public enum ChecksumAlgorithm
    {
        MD5,
        SHA1,
        SHA256,
        SHA384,
        SHA512
    }

    public static class ChecksumAlgorithmMap
    {
        public static Dictionary<string, ChecksumAlgorithm> Algorithms = new()
        {
            { "md5", ChecksumAlgorithm.MD5 },
            { "sha1", ChecksumAlgorithm.SHA1 },
            { "sha256", ChecksumAlgorithm.SHA256 },
            { "sha384", ChecksumAlgorithm.SHA384 },
            { "sha512", ChecksumAlgorithm.SHA512 }
        };

        public static Dictionary<ChecksumAlgorithm, string> AlgorthmStrings = new()
        {
            {ChecksumAlgorithm.MD5, "md5"},
            {ChecksumAlgorithm.SHA1, "sha1"},
            {ChecksumAlgorithm.SHA256, "sha256"},
            {ChecksumAlgorithm.SHA384, "sha384"},
            {ChecksumAlgorithm.SHA512, "sha512"}
        };
    }
}
