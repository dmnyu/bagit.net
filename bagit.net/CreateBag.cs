using System;
using System.IO;

namespace bagit.net
{
    public class Bagit
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
            tempDataDir = Path.Combine(bagLocation, Guid.NewGuid().ToString());

            if (!Directory.Exists(tempDataDir))
                Directory.CreateDirectory(tempDataDir);
        }

        internal void MoveContentsToTemp()
        {
            // Move all top-level files
            foreach (var file in Directory.GetFiles(bagLocation))
            {
                var dest = Path.Combine(tempDataDir, Path.GetFileName(file));
                File.Move(file, dest);
            }

            // Move all top-level directories except the temp folder itself
            foreach (var dir in Directory.GetDirectories(bagLocation))
            {
                if (dir.Equals(tempDataDir, StringComparison.OrdinalIgnoreCase))
                    continue;

                var destDir = Path.Combine(tempDataDir, Path.GetFileName(dir));
                Directory.Move(dir, destDir);
            }
        }

        internal void MoveTempToDataDir()
        {
            Directory.Move(tempDataDir, dataDir);
        }
    }
}
