using bagit.net.domain;

namespace bagit.net.interfaces
{
    public interface IValidationService
    {
        void ValidateBag(string bagPath);
        void ValidateBagFast(string bagPath);
        void ValidateBagCompleteness(string bagPath);
        bool HasRequiredFiles(string bagPath);
    }
}
