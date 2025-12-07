using bagit.net.domain;
using bagit.net.interfaces;
using Microsoft.Extensions.Logging;

namespace bagit.net.cli.lib
{
    public class Manager
    {
        private readonly ILogger _logger;
        private readonly IMessageService _messageService;
        public Manager(ILogger<Validator> logger, IValidationService validationService, IMessageService messageService)
        {
            _logger = logger;
            _messageService = messageService;
           
        }

        public void Add(string bagPath, string kv)
        {
            var (valid, key, value) = parseKeyValue(kv);
            if (!valid) 
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"`{kv}` is not valid"));
            }

            var bagInfo = Path.Combine(Path.GetDirectoryName(bagPath), "bag-info.txt");
            _messageService.Add(new MessageRecord(MessageLevel.INFO, $"adding key-value `{key}: {value}` to {bagInfo}"));
        }

        public void Set(string bagPath, string kv)
        {
            var (valid, key, value) = parseKeyValue(kv);
            if (!valid)
            {
                _messageService.Add(new MessageRecord(MessageLevel.ERROR, $"`{kv}` is not valid"));
            }

            var bagInfo = Path.Combine(Path.GetDirectoryName(bagPath), "bag-info.txt");
            _messageService.Add(new MessageRecord(MessageLevel.INFO, $"setting key-value `{key}: {value}` to {bagInfo}"));

            
        }

        public void Delete(string bagPath, string key)
        {
            var bagInfo = Path.Combine(Path.GetDirectoryName(bagPath), "bag-info.txt");
            _messageService.Add(new MessageRecord(MessageLevel.INFO, $"deleting `{key}` from {bagInfo}"));
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
