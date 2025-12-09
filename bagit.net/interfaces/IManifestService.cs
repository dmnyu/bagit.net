using bagit.net.domain;

namespace bagit.net.interfaces
{
    public interface IManifestService
    {
        void CreatePayloadManifest(string bagRoot, IEnumerable<ChecksumAlgorithm> algorithms);
        void CreateTagManifestFile(string bagRoot, IEnumerable<ChecksumAlgorithm> algorithms);
        List<KeyValuePair<string, string>> GetManifestAsKeyValuePairs(string manifestPath);
        void ValidateManifestFiles(string bagRoot);
        void ValidateManifestFile(string manifestFile);
        void ValidateManifestFilesCompleteness(string bagRoot);
        void ValidateManifestFileCompleteness(string manifestFile);
        void UpdateTagManifest(string bagRoot);
    }
}
