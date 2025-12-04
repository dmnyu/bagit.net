using bagit.net.domain;

namespace bagit.net.interfaces
{
    public interface IManifestService
    {
        void CreatePayloadManifest(string bagRoot, ChecksumAlgorithm algorithm);
        void CreateTagManifestFile(string bagRoot, ChecksumAlgorithm algorithm);
        List<KeyValuePair<string, string>> GetManifestAsKeyValuePairs(string manifestPath);
        void ValidateManifestFiles(string bagRoot);
        void ValidateManifestFile(string manifestFile);
        void ValidateManifestFilesCompleteness(string bagRoot);
        void ValidateManifestFileCompleteness(string manifestFile);
    }
}
