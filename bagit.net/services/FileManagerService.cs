using bagit.net.interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

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
            File.Create(path).Dispose();
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

        public bool HasBOM(string path)
        {
                Span<byte> bom = stackalloc byte[3];

                using var fs = File.OpenRead(path);
                if (fs.Read(bom) < 3)
                    return false;

                return bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF;
        }

        public bool IsValidUTF8(string path)
        {
            var utf8 = new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true);

            byte[] bytes = File.ReadAllBytes(path);

            try
            {
                utf8.GetString(bytes);
                return true;
            }
            catch (DecoderFallbackException)
            {
                return false;
            }
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
