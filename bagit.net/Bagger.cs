using Microsoft.Extensions.Logging;
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

            if (path == null)
            {
                Bagit.Logger.LogCritical("The path to a directory cannot be null");
                throw new ArgumentNullException(nameof(path),"The path to a directory cannot be null");
            }
            if (!Directory.Exists(path))
            {
                Bagit.Logger.LogCritical("{path} does not exist", path);
                throw new DirectoryNotFoundException($"{path} does not exist");
            }

            Bagit.Logger.LogInformation("Creating bag for directory {path}", path);
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
                Manifest.CreateTagManifestFile(bagLocation, algorithm);
            }
            catch (IOException ex)
            {
                Bagit.Logger.LogCritical(ex, "Failed to create bag at {Path}", path);
                throw new InvalidOperationException($"Failed to create bag at {path}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Bagit.Logger.LogCritical(ex, "Access denied when creating bag at {path}", path);
                throw new InvalidOperationException($"Access denied when creating bag at {path}", ex);
            }
            catch (Exception ex){
                Bagit.Logger.LogCritical(ex, "Unknown exception while creating bag at {path}", path);
                throw new Exception($"Unknown exception while creating bag at {path}", ex);
            }
        }

        internal void CreateTempDataDir()
        {
            Bagit.Logger.LogInformation($"Creating {dataDir}");
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
                Bagit.Logger.LogInformation("Moving {filename} to {dest}", filename, dest);
                File.Move(file, dest);
            }

            // Move all top-level directories except the temp folder itself
            foreach (var dir in Directory.GetDirectories(bagLocation))
            {
                if (dir.Equals(tempDataDir, StringComparison.OrdinalIgnoreCase))
                    continue;

                var dirName = Path.GetFileName(dir);
                var destDir = Path.Combine(tempDataDir, dirName);
                Bagit.Logger.LogInformation("Moving {dirName} to {destDir}", dirName, destDir);
                Directory.Move(dir, destDir);
            }
        }

        internal void MoveTempToDataDir()
        {
            var tmpName = Path.GetFileName(tempDataDir);
            Bagit.Logger.LogInformation("Moving {tempDataDir} to data", tempDataDir);
            Directory.Move(tempDataDir, dataDir);
        }

        internal void CreateBagitTXT()
        {
            Bagit.Logger.LogInformation("Creating bagit.txt");
            var bagitTxt = Path.Combine(bagLocation, "bagit.txt");
            if (!System.Text.RegularExpressions.Regex.IsMatch(Bagit.BAGIT_VERSION, @"^\d+\.\d+$"))
            {
                Bagit.Logger.LogCritical("Invalid BagIt version: {b}. Must be in 'major.minor' format.", Bagit.BAGIT_VERSION);
                throw new InvalidOperationException($"Invalid BagIt version: {Bagit.BAGIT_VERSION}. Must be in 'major.minor' format.");
            }
            var content = $"BagIt-Version: {Bagit.BAGIT_VERSION}{nl}Tag-File-Character-Encoding: UTF-8{nl}";
            File.WriteAllText(bagitTxt, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
    }
}
