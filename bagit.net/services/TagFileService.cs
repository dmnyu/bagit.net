using bagit.net.domain;
using bagit.net.interfaces;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace bagit.net.services
{
    public class TagFileService : ITagFileService
    {
        private readonly IFileManagerService _fileManagerService;
        private readonly IMessageService _messageService;
        private static readonly Regex _oxumPattern = new(@"^\d+\.\d+$", RegexOptions.Compiled);

        public TagFileService(IFileManagerService fileManagerService, IMessageService messageService)
        {
            _fileManagerService = fileManagerService;
            _messageService = messageService;
        }

        public Dictionary<string, string> GetTagFileAsDict(string tagFilePath)
        {
            var tagDictionary = new Dictionary<string, string>();
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

        public void CreateBagInfo(string bagDir, string? tagFileLocation)
        {
            var oxum = CalculateOxum(bagDir);
            var sb = new StringBuilder();

            sb.Append($"Bag-Software-Agent: bagit.net v{Bagit.VERSION}\n");
            sb.Append($"BagIt-Version: {Bagit.BAGIT_VERSION}\n");
            sb.Append($"Bagging-Date: {DateTime.UtcNow:yyyy-MM-dd}\n");
            sb.Append($"Payload-Oxum: {oxum}\n");

            if( tagFileLocation != null )
            {
                var tagFileName = Path.GetFileName(tagFileLocation);
                _messageService.Add(new MessageRecord(MessageLevel.INFO, $"Adding metadata file {tagFileName} to bag-info.txt"));
                var tags = GetTags(tagFileLocation);
                foreach( var tag in tags)
                {
                    foreach(var val in tag.Value)
                    {
                        sb.Append($"{tag.Key}: {val}\n");
                    }
                }
            }

            var bagInfoFile = Path.Combine(bagDir, "bag-info.txt");
            File.WriteAllText(bagInfoFile, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        public void CreateBagItTXT(string bagRoot)
        {
            var bagitTxt = Path.Combine(bagRoot, "bagit.txt");
            if (!System.Text.RegularExpressions.Regex.IsMatch(Bagit.BAGIT_VERSION, @"^\d+\.\d+$"))
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"Invalid BagIt version: {Bagit.BAGIT_VERSION}. Must be in 'major.minor' format."));
                return;
            }
            var content = $"BagIt-Version: {Bagit.BAGIT_VERSION}\nTag-File-Character-Encoding: UTF-8\n";
            File.WriteAllText(bagitTxt, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        public string CalculateOxum(string bagRoot)
        {
            string dataDir = Path.Combine(bagRoot, "data");
            int count = 0;
            long numBytes = 0;

            if (!Directory.Exists(dataDir))
                throw new ArgumentException($"Data directory does not exist: {dataDir}");

            foreach (var file in Directory.EnumerateFiles(dataDir, "*", SearchOption.AllDirectories))
            {
                var info = new FileInfo(file);
                numBytes += info.Length;
                count++;
            }

            return $"{numBytes}.{count}";
        }

        public List<KeyValuePair<string, string>> GetTagFileAsList(string baginfoPath)
        {
            return File.ReadAllLines(baginfoPath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line =>
                {
                    var parts = line.Split(": ", 2);
                    if (parts.Length != 2)
                        throw new FormatException($"Invalid bag-info.txt line: {line}");
                    return new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim());
                })
                .ToList();
        }

        public void ValidateBagitTXT(string bagRoot)
        {
            var bagitPath = Path.Combine(bagRoot, "bagit.txt");
            if (!File.Exists(bagitPath))
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"bagit.txt is missing from {bagRoot}."));
                return;
            }
            var tags = GetTagFileAsDict(bagitPath);

            if (!tags.TryGetValue("BagIt-Version", out var version))
            {
                _messageService.Add(new MessageRecord(MessageLevel.WARNING, ("BagIt-Version key is missing in bagit.txt.")));
            } else {
                if (!System.Text.RegularExpressions.Regex.IsMatch(version, @"^\d+\.\d+$"))
                    _messageService.Add(new MessageRecord(MessageLevel.WARNING, ($"Invalid BagIt-Version format: {version}")));
            }

            if (!tags.TryGetValue("Tag-File-Character-Encoding", out var encoding))
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, "Tag-File-Character-Encoding key is missing in bagit.txt."));
            }
            else
            {
                if (!string.Equals(encoding, "UTF-8", StringComparison.OrdinalIgnoreCase))
                {
                    _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"Unsupported Tag-File-Character-Encoding: {encoding}"));
                }
            }
        }

        public bool HasBagInfo(string bagRoot)
        {
            if(!Path.Exists(Path.Combine(bagRoot, "bag-info.txt"))) 
            {
                return false;
            } 
            return true;    
        }

        public bool HasBagItTXT(string bagRoot)
        {
            if (!Path.Exists(Path.Combine(bagRoot, "bagit.txt")))
            {
                return false;
            }
            return true;
        }

        public void ValidateBagInfo(string bagInfoPath)
        {
            //check that it is valid UTF8
            if (!_fileManagerService.IsValidUTF8(bagInfoPath))
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, "bag-info.txt is not valid UTF-8 encoded text"));

            //check that it does not contain a BOM
            if (_fileManagerService.HasBOM(bagInfoPath))
                _messageService.Add(new MessageRecord(MessageLevel.WARNING, "bag-info.txt contains BOM byte sequence"));

            //check that the tag file does not contain invalid control characters
            ScanFileForInvalidControlChars(bagInfoPath);

            //validate tags
            ValidateTags(bagInfoPath);
        }

        public void ValidateTags(string bagInfoPath)
        {
            var _tags = GetTags(bagInfoPath);

            //validate the oxum
            if (_tags.TryGetValue("Payload-Oxum", out var payloadOxum))
            {
                // More than one = invalid
                if (payloadOxum.Count > 1)
                    _messageService.Add(new MessageRecord(MessageLevel.ERROR, "bag-info.txt contains multiple payload oxum tags, cannot validate"));
                else
                {
                    // if there is a oxum, ensure it matches 
                    var oxum = payloadOxum[0];
                    if (string.IsNullOrEmpty(oxum))
                        _messageService.Add(new MessageRecord(MessageLevel.WARNING, "bag-info.txt does not contain a payload-oxum"));
                    else
                    {
                        if (!_oxumPattern.IsMatch(oxum))
                            _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"bag-info.txt payload-oxum `{oxum}` value is invalid"));
                        else
                        {
                            var bagRoot = Path.GetDirectoryName(bagInfoPath);
                            var calculatedOxum = CalculateOxum(bagRoot!);
                            //validate the oxum
                            if (oxum != calculatedOxum)
                                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"payload oxum is invalid, stored: {oxum} calculated: {calculatedOxum}"));
                        }
                    }
                }
            }

            if (_tags.TryGetValue("Bag-Software-Agent", out var agent) && agent.Count > 1)
                _messageService.Add(new MessageRecord(MessageLevel.WARNING, $"bag-info.txt contains multiple Bag-Software-Agent tags"));

            if (_tags.TryGetValue("Bagging-Date", out var baggingDate))
            {

                if (baggingDate.Count > 1)
                    _messageService.Add(new MessageRecord(MessageLevel.WARNING, $"bag-info.txt contains multiple Bagging-Date tags"));
                else
                {
                    var date = baggingDate[0];
                    if (string.IsNullOrEmpty(date))
                        _messageService.Add(new MessageRecord(MessageLevel.WARNING, $"bag-info.txt does not contain a Bagging-Date tag"));
                    else
                    {

                        if (!DateTime.TryParseExact(
                            date,
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out _))
                        {
                            _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"bag-info.txt Bagging-Date must be in YYYY-MM-DD format (ISO 8601 date)"));
                        }
                    }
                }
            }
            
        }

        public Dictionary<string, List<string>> GetTags(string tagFilePath)
        {
            var _tags = new Dictionary<string, List<string>>();

            string? currentKey = null;

            foreach (var line in File.ReadLines(tagFilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (char.IsWhiteSpace(line[0]))
                {
                    if (currentKey == null)
                        throw new InvalidDataException("Continuation line with no preceding key.");

                    _tags[currentKey].Add(line.TrimStart());
                }
                else
                {
                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length != 2)
                        throw new InvalidDataException($"Invalid line: {line}");

                    currentKey = parts[0].Trim();
                    var value = parts[1].Trim();

                    if (!_tags.ContainsKey(currentKey))
                        _tags[currentKey] = new List<string>();

                    _tags[currentKey].Add(value);
                }
            }
            return _tags;
        }

        public void ScanFileForInvalidControlChars(string path)
        {
            int lineNum = 0;

            foreach (var line in File.ReadLines(path))
            {
                lineNum++;
                foreach (var ch in line)
                {
                    if (char.IsControl(ch) && ch != '\r' && ch != '\n' && ch != '\t')
                    {
                       _messageService.Add(new MessageRecord(
                            MessageLevel.ERROR,
                            $"Control character 0x{(int)ch:X2} found on line {lineNum} in {path}"
                        ));
                    }
                }
            }
        }
    }
}
