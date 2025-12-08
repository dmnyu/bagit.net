using bagit.net.domain;
using bagit.net.interfaces;
using Microsoft.Extensions.Logging;

namespace bagit.net.cli.lib
{
    public class TagManager
    {
        private readonly ILogger _logger;
        private readonly IMessageService _messageService;
        private readonly ITagFileService _tagFileService;
        public TagManager(ILogger<BagValidator> logger, IValidationService validationService, IMessageService messageService, ITagFileService tagFileService)
        {
            _logger = logger;
            _messageService = messageService;
            _tagFileService = tagFileService;
        }

        public void Add(string bagPath, string kv)
        {
            var (valid, key, value) = parseKeyValue(kv);
            if (!valid || key is null || value is null) 
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"`{kv}` is not valid"));
                return;
            }

            var bagInfo = Path.Combine(bagPath, "bag-info.txt");
            _tagFileService.AddTag(key, value, bagPath);
        }

        public void Set(string bagPath, string kv)
        {
            var (valid, key, value) = parseKeyValue(kv);
            if (!valid || key is null || value is null)
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"`{kv}` is not valid"));
                return;
            }

            var dir = Path.GetDirectoryName(bagPath);
            if (string.IsNullOrEmpty(dir))
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"Cannot determine directory for bag path `{bagPath}`."));
                return;
            }
            var bagInfo = Path.Combine(bagPath, "bag-info.txt");
            _tagFileService.SetTag(key, value, bagPath);
        }

        public void Delete(string bagPath, string key)
        {
            var dir = Path.GetDirectoryName(bagPath);
            if (string.IsNullOrEmpty(dir))
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"Cannot determine directory for bag path `{bagPath}`."));
                return;
            }
            var bagInfo = Path.Combine(dir, "bag-info.txt");
            _messageService.Add(new MessageRecord(MessageLevel.INFO, $"deleting `{key}` from {bagInfo}"));
            _tagFileService.DeleteTag(key, bagPath);
        }

        public void View(string bagPath)
        {
            _tagFileService.ViewTagFile(bagPath);
        }

        public (bool valid, string? key, string? value) parseKeyValue(string input)
        {
            var index = input.IndexOf('=');
            if (index <= 0 || index == input.Length - 1)
                return (false, null, null);
            var key = input.Substring(0, index).Trim();
            var value = input.Substring(index + 1).Trim();
            return (true, key, value);
        }

        public void LogMessages()
        {
            Logging.LogEvents(_messageService.GetAll(), false, _logger);
        }
    }
}
