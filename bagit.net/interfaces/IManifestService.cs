using bagit.net.domain;

namespace bagit.net.interfaces
{
    public interface IManifestService
    {
        void CreatePayloadManifest(string bagRoot, ChecksumAlgorithm algorithm);
        void CreateTagManifestFile(string bagRoot, ChecksumAlgorithm algorithm);
        List<KeyValuePair<string, string>> GetManifestAsKeyValuePairs(string manifestPath);
        IEnumerable<MessageRecord> ValidateManifestFiles(string bagRoot);
        IEnumerable<MessageRecord> ValidateManifestFile(string manifestFile);
        IEnumerable<MessageRecord> ValidateManifestFilesCompleteness(string bagRoot);
        IEnumerable<MessageRecord> ValidateManifestFileCompleteness(string manifestFile);
    }
}
