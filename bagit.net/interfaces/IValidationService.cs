using bagit.net.domain;

namespace bagit.net.interfaces
{
    public interface IValidationService
    {
        Task ValidateBag(string bagPath, int processes);
        void ValidateBagFast(string bagPath);
        void ValidateBagCompleteness(string bagPath);
        bool HasRequiredFiles(string bagPath);
    }
}
