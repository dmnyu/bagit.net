namespace bagit.net.interfaces
{
    public interface IFileManagerService
    {
        void CreateDirectory(string path);
        void CreateFile(string path);
        void DeleteFile(string path);
        string CreateTempDirectory(string path);
        string CreateTempFile(string path);
        void MoveDirectory(string originalPath, string destinationPath);
        void MoveFile(string originalPath, string destinationPath);
        void MoveContentsOfDirectory(string originalPath, string destinationPath);
        bool IsValidUTF8(string path);
        bool HasBOM(string path);
        void WriteToFile(string file, string content);
    }
}
