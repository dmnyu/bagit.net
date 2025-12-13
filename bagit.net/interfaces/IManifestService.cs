using bagit.net.domain;

namespace bagit.net.interfaces
{
    public interface IManifestService
    {
        Task CreatePayloadManifest(string bagRoot, IEnumerable<ChecksumAlgorithm> algorithms, int processes);
        void CreateTagManifestFile(string bagRoot, IEnumerable<ChecksumAlgorithm> algorithms);
        List<KeyValuePair<string, string>> GetManifestAsKeyValuePairs(string manifestPath);
        //Task ValidateManifestFile(string manifestFile, int processes);
        Task ValidateManifestFiles(string bagPath, int processes);
        (string payloadFile, string hash) ValidateManifestLine(string line);
        void ValidateManifestFilesCompleteness(string bagRoot);
        void UpdateTagManifest(string bagRoot);
    }
}
