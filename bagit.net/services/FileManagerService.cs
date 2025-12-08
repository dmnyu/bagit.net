using bagit.net.domain;
using bagit.net.interfaces;
using System.Text;

namespace bagit.net.services
{
    public class FileManagerService : IFileManagerService
    {
        private readonly IMessageService _messageService;
        private readonly string _tmpChars = "abcdefghijklmnopqrstuvwxyz0123456789";

        public FileManagerService(IMessageService messageService)
        {
            _messageService = messageService;
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
            var random = new Random();
            var suffix = new string(Enumerable.Range(0, 8)
                .Select(_ => _tmpChars[random.Next(_tmpChars.Length)])
                .ToArray());
            var tempDataDir = Path.Combine(path, $"tmp{suffix}");
            CreateDirectory(tempDataDir);
            return tempDataDir;
        }

        public string CreateTempFile(string path)
        {
            var random = new Random();
            var suffix = new string(Enumerable.Range(0, 8)
                .Select(_ => _tmpChars[random.Next(_tmpChars.Length)])
                .ToArray());
            var tmpFile = Path.Combine(path, $"tmp{suffix}");
            CreateFile(tmpFile);
            return tmpFile;
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
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
                _messageService.Add(new MessageRecord(MessageLevel.INFO, $"Moving {filename} to {dest}"));
                File.Move(file, dest);
            }

            foreach (var dir in Directory.GetDirectories(originalPath))
            {
                if (dir.Equals(destinationPath, StringComparison.OrdinalIgnoreCase))
                    continue;

                var dirName = Path.GetFileName(dir);
                var destDir = Path.Combine(destinationPath, dirName);
                _messageService.Add(new MessageRecord(MessageLevel.INFO, $"Moving {dirName} to {destDir}"));
                MoveDirectory(dir, destDir);
            }
        }

        public void MoveDirectory(string originalPath, string destinationPath)
        {
            Directory.Move(originalPath, destinationPath);
        }

        public void MoveFile(string originalPath, string destinationPath)
        {
            File.Move(originalPath, destinationPath);
        }

        public void WriteToFile(string path, string content)
        {
            File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
    }
}
