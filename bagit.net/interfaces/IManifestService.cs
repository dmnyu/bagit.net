using bagit.net.services;

namespace bagit.net.interfaces
{
    public interface IManifestService
    {
        void CreatePayloadManifest(string bagRoot, ChecksumAlgorithm algorithm);
        void CreateTagManifestFile(string bagRoot, ChecksumAlgorithm algorithm);
        List<KeyValuePair<string, string>> GetManifestAsKeyValuePairs(string manifestPath);
        void ValidateManifestFile(string manifestFile);
    }
}
