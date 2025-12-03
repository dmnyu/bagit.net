using bagit.net.domain;

namespace bagit.net.interfaces
{
    public interface IValidationService
    {
        void ValidateBag(string bagPath);
        MessageRecord ValidateBagFast(string bagPath);
        IEnumerable<MessageRecord> ValidateBagCompleteness(string bagPath);
        void HasRequiredFiles(string bagPath);
    }
}
