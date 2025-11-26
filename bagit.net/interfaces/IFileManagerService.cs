namespace bagit.net.interfaces
{
    public interface IFileManagerService
    {
        void CreateDirectory(string path);
        string CreateTempDirectory(string path);
        void MoveDirectory(string originalPath, string destinationPath);
        void MoveContentsOfDirectory(string originalPath, string destinationPath);
        void CreateFile(string path);
        bool IsValidUTF8(string path);
        bool HasBOM(string path);
    }
}
