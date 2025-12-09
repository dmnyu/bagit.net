using bagit.net.domain;

namespace bagit.net.interfaces
{
    public interface ICreationService
    {
        public void CreateBag(string bagPath, IEnumerable<ChecksumAlgorithm> algorithms, string? tagFileLocation);
    }
}
