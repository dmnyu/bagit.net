using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace bagit.net
{
    public class Bagger
    {
        private string bagLocation = string.Empty;
        private string dataDir = string.Empty;
        private string tempDataDir = string.Empty;
        private string nl = Environment.NewLine;

        public void CreateBag(string? path, ChecksumAlgorithm algorithm)
        {
            if (path == null) throw new ArgumentNullException(nameof(path), "The path to a directory cannot be null");
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"{path} does not exist");

            bagLocation = path;
            dataDir = Path.Combine(bagLocation, "data");

            try
            {
                CreateTempDataDir();
                MoveContentsToTemp();
                MoveTempToDataDir();
                Manifest.CreatePayloadManifest(bagLocation, algorithm);
                CreateBagitTXT();
                BagInfo.CreateBagInfo(bagLocation);
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
            Console.WriteLine($"Moving {tempDataDir} to data");
            Directory.Move(tempDataDir, dataDir);
        }

        internal void CreateBagitTXT()
        {
            Console.WriteLine("Creating bagit.txt");
            var bagitTxt = Path.Combine(bagLocation, "bagit.txt");
            if (!System.Text.RegularExpressions.Regex.IsMatch(Bagit.BAGIT_VERSION, @"^\d+\.\d+$"))
                throw new InvalidOperationException($"Invalid BagIt version: {Bagit.BAGIT_VERSION}. Must be in 'major.minor' format.");

            var content = $"BagIt-Version: {Bagit.BAGIT_VERSION}{nl}Tag-File-Character-Encoding: UTF-8{nl}";
            File.WriteAllText(bagitTxt, content, Encoding.UTF8);
        }
    }
}
