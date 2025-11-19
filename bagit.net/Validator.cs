using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace bagit.net
{
    public class Validator
    {

        internal void Has_Valid_BagitTXT(string path)
        {
            var bagitPath = Path.Combine(path, "bagit.txt");
            if (!File.Exists(bagitPath))
                throw new FileNotFoundException("bagit.txt is missing from the bag root.", bagitPath);

            var tags = TagFile.GetTagFileAsDict(bagitPath);

            if (!tags.TryGetValue("BagIt-Version", out var version))
                throw new FormatException("BagIt-Version key is missing in bagit.txt.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(version, @"^\d+\.\d+$"))
                throw new FormatException($"Invalid BagIt-Version format: {version}");

            if (!tags.TryGetValue("Tag-File-Character-Encoding", out var encoding))
                throw new FormatException("Tag-File-Character-Encoding key is missing in bagit.txt.");

            if (!string.Equals(encoding, "UTF-8", StringComparison.OrdinalIgnoreCase))
                throw new FormatException($"Unsupported Tag-File-Character-Encoding: {encoding}");
        }

        internal void Has_Valid_BaginfoTXT(string path)
        {
            var bagInfoFile = Path.Combine(path, "bag-info.txt");
            if (!Path.Exists(bagInfoFile))
            {
                Bagit.Logger.LogWarning("{d} does not contain a bag-info.txt file.", Path.GetDirectoryName(path));
                return;
            }

            var tags = TagFile.GetTagFileAsDict(bagInfoFile);
            if (!tags.TryGetValue("File-Oxum", out var version))
            {
                Bagit.Logger.LogWarning("bag-info.txt does not contain a File-Oxum, skipping validation");
            }
            else
            {
                var oxum = BagInfo.GetOxum(path);
                if (oxum != tags["File-Oxum"])
                {
                    throw new InvalidDataException("File-Oxum in bag-info.txt does not match.");
                }
            }
        }
    }
}
