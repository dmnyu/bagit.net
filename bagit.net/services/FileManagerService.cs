using bagit.net.interfaces;
using Microsoft.Extensions.Logging;

namespace bagit.net.services
{
    public class FileManagerService : IFileManagerService
    {
        private readonly ILogger _logger;

        public FileManagerService(ILogger<FileManagerService> logger)
        {
            _logger = logger;
        }
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void CreateFile(string path)
        {
            File.Create(path);
        }

        public string CreateTempDirectory(string path)
        {
            
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var suffix = new string(Enumerable.Range(0, 8)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
            var tempDataDir = Path.Combine(path, $"tmp{suffix}");
            CreateDirectory(tempDataDir);
            return tempDataDir;
        }

        public void MoveContentsOfDirectory(string originalPath, string destinationPath)
        {
            foreach (var file in Directory.GetFiles(originalPath))
            {
                var filename = Path.GetFileName(file);
                var dest = Path.Combine(destinationPath, filename);
                _logger.LogInformation("Moving {filename} to {dest}", filename, dest);
                File.Move(file, dest);
            }

            foreach (var dir in Directory.GetDirectories(originalPath))
            {
                if (dir.Equals(destinationPath, StringComparison.OrdinalIgnoreCase))
                    continue;

                var dirName = Path.GetFileName(dir);
                var destDir = Path.Combine(destinationPath, dirName);
                _logger.LogInformation("Moving {dirName} to {destDir}", dirName, destDir);
                MoveDirectory(dir, destDir);
            }
        }

        public void MoveDirectory(string originalPath, string destinationPath)
        {
            Directory.Move(originalPath, destinationPath);
        }
    }
}
