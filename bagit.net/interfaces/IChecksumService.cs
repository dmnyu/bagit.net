using bagit.net.domain;

namespace bagit.net.interfaces
{
    public interface IChecksumService
    {
        Task<Dictionary<ChecksumAlgorithm, string>> CalculateChecksums(string path, IEnumerable<ChecksumAlgorithm> algorithms);
        Task<string> CalculateChecksum(string filePath, ChecksumAlgorithm algorithm);
        Task<bool> CompareChecksum(string filePath, string expectedChecksum, ChecksumAlgorithm algorithm);
        Task CompareChecksums(string payloadPath, Dictionary<ChecksumAlgorithm, string> hashes, int processes);
        string GetAlgorithmCode(ChecksumAlgorithm algorithm);
    }
}
