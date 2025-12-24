using bagit.net.domain;

namespace bagit.net.interfaces
{
    public interface ICreationService
    {
        public Task CreateBag(string bagPath, IEnumerable<ChecksumAlgorithm> algorithms, string? tagFileLocation, int processes);
    }
}
