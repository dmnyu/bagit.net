namespace bagit.net.interfaces
{
    public interface IValidationService
    {
        void ValidateBag(string bagPath);
        void ValidateBagFast(string bagPath);
        void ValidateCompleteness(string bagPath);
        void HasRequiredFiles(string bagPath);
    }
}
