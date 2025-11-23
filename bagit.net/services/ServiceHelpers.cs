namespace bagit.net.services
{
    public static class ServiceHelpers
    {
        public const string ChecksumPattern = @"-(md5|sha1|sha256|sha384|sha512)\b";
        public const string ManifestPattern = @"manifest-(md5|sha1|sha256|sha384|sha512).txt";
        public const string TagmanifestPattern = @"tagmanifest-(md5|sha1|sha256|sha384|sha512).txt";
    }
}
