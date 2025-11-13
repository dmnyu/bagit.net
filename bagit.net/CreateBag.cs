namespace bagit.net
{
    public class Bagit
    {
        private string bagLocation = string.Empty;
        private string dataDir = string.Empty;

        public void CreateBag(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"{path} does not exist");

            bagLocation = path;
            createDataDir();
            moveFiles();
        }

        internal void createDataDir()
        {
            dataDir = Path.Combine(bagLocation, Guid.NewGuid().ToString());

            if (!Directory.Exists(dataDir))
                Directory.CreateDirectory(dataDir);
        }

        internal void moveFiles()
        {
            // Move all top-level files into the temp data folder
            foreach (var file in Directory.GetFiles(bagLocation))
            {
                File.Move(file, Path.Combine(dataDir, Path.GetFileName(file)));
            }

            // Move all subdirectories except the temp folder itself into the temp folder
            foreach (var dir in Directory.GetDirectories(bagLocation))
            {
                // Skip the temp folder
                if (dir.Equals(dataDir, StringComparison.OrdinalIgnoreCase))
                    continue;

                var destDir = Path.Combine(dataDir, Path.GetFileName(dir));
                Directory.Move(dir, destDir);
            }

            // Rename the temp GUID folder to "data"
            var finalDataDir = Path.Combine(bagLocation, "data");

            Directory.Move(dataDir, finalDataDir);

            // Update internal reference
            dataDir = finalDataDir;
        }
    }
}