namespace bagit.net.domain
{
    public static class Bagit
    {
        public const string VERSION = "0.2.7-alpha"; 
        public const string BAGIT_VERSION = "1.0";

    }

    public static class BagitContext
    {
        public static readonly AsyncLocal<bool> Quiet = new();
        public static readonly AsyncLocal<int> BufferSize = new();
    }
}
