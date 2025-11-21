using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace bagit.net
{
    public static class TagFile
    {
        public static Dictionary<String, String> GetTagFileAsDict(string tagFilePath)
        {
            var tagDictionary = new Dictionary<String, String>();
            foreach (var line in File.ReadAllLines(tagFilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(": ", 2, StringSplitOptions.None);

                if (parts.Length != 2)
                    throw new FormatException($"Invalid tag file line: {line}");
                if (tagDictionary.ContainsKey(parts[0]))
                    throw new FormatException($"tag file contains duplicate key {parts[0]}");
                tagDictionary.Add(parts[0], parts[1]);
            }

            return tagDictionary;
        }
    }
}
