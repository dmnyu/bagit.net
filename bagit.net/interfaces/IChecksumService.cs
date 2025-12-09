using bagit.net.domain;

namespace bagit.net.interfaces
{
    public interface IChecksumService
    {      
        Task<string> CalculateChecksum(string filePath, ChecksumAlgorithm algorithm);
        bool CompareChecksum(string filePath, string expectedChecksum, ChecksumAlgorithm algorithm);
        string GetAlgorithmCode(ChecksumAlgorithm algorithm);
    }
}
