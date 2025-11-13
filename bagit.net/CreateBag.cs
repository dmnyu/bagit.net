using System;
using System.IO;

namespace bagit.net
{
    public class Bagger
    {
        private string bagLocation = string.Empty;
        private string dataDir = string.Empty;
        private string tempDataDir = string.Empty;

        public void CreateBag(string path)
        {
            ArgumentNullException.ThrowIfNull(path);

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"{path} does not exist");

            bagLocation = path;
            dataDir = Path.Combine(bagLocation, "data");

            if (Directory.Exists(dataDir))
                throw new IOException($"Target data directory already exists: {dataDir}");

            try
            {
                CreateTempDataDir();
                MoveContentsToTemp();
                MoveTempToDataDir();
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"Failed to create bag at {path}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException($"Access denied when creating bag at {path}", ex);
            }
        }

        internal void CreateTempDataDir()
        {
            Console.WriteLine("Creating data directory");
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var suffix = new string(Enumerable.Range(0, 8)
                                             .Select(_ => chars[random.Next(chars.Length)])
                                             .ToArray());

            tempDataDir = Path.Combine(bagLocation, $"tmp{suffix}");
            Directory.CreateDirectory(tempDataDir);
        }

        internal void MoveContentsToTemp()
        {
            // Move all top-level files
            foreach (var file in Directory.GetFiles(bagLocation))
            {
                var filename = Path.GetFileName(file);
                var dest = Path.Combine(tempDataDir, filename);
                Console.WriteLine($"Moving {filename} to {dest}");
                File.Move(file, dest);
            }

            // Move all top-level directories except the temp folder itself
            foreach (var dir in Directory.GetDirectories(bagLocation))
            {
                if (dir.Equals(tempDataDir, StringComparison.OrdinalIgnoreCase))
                    continue;

                var dirName = Path.GetFileName(dir);
                var destDir = Path.Combine(tempDataDir, dirName);
                Console.WriteLine($"Moving {dirName} to {destDir}");
                Directory.Move(dir, destDir);
            }
        }

        internal void MoveTempToDataDir()
        {
            var tmpName = Path.GetFileName(tempDataDir);
            Console.WriteLine($"Moving {tmpName} to data");
            Directory.Move(tempDataDir, dataDir);
        }
    }
}
