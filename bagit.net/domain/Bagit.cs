namespace bagit.net.domain
{
    public static class Bagit
    {
        public const string VERSION = "0.3.0-beta"; 
        public const string BAGIT_VERSION = "1.0";

    }

    public static class Options
    {
        public static readonly AsyncLocal<bool> Quiet = new();
        public static readonly AsyncLocal<int> BufferSize = new();
        public static readonly AsyncLocal<IEnumerable<ChecksumAlgorithm>> ChecksumAlgorithms = new();
        public static readonly AsyncLocal<string?> LogFile = new();
        public static readonly AsyncLocal<string?> TagFile = new();
        public static readonly AsyncLocal<int> Processes = new();
        public static readonly AsyncLocal<string> Directory = new();
        public static readonly AsyncLocal<CancellationToken> CancellationToken = new();

    }
}
