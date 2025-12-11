using bagit.net.domain;
using bagit.net.interfaces;
using Microsoft.Extensions.Logging;

namespace bagit.net.services
{
    public class MessageService : IMessageService
    {
        private readonly ILogger _logger;
        public MessageService(ILogger<MessageService> logger) {
            _logger = logger;
        }

        private readonly List<MessageRecord> _messages = new();
        public void Add(MessageRecord message)
        {
            _messages.Add(message);
            LogEvent(message);
        }
        public void AddRange(IEnumerable<MessageRecord> messages) => _messages.AddRange(messages);
        public IReadOnlyList<MessageRecord> GetAll() => _messages.AsReadOnly();
        public void Clear() => _messages.Clear();

        public void LogEvent(MessageRecord messageRecord)
        {
            bool quiet = MessageContext.Quiet.Value;

            switch (messageRecord.GetLevel())
            {
                case MessageLevel.INFO:
                    if (!quiet) _logger.LogInformation(messageRecord.GetMessage());
                    break;
                case MessageLevel.ERROR:
                    _logger.LogError(messageRecord.GetMessage());
                    break;
                case MessageLevel.WARNING:
                    _logger.LogWarning(messageRecord.GetMessage());
                    break;
                default:
                    throw new InvalidDataException("Unknown message level");
            }
        }

        public void LogEvents(IEnumerable<MessageRecord> records)
        {
            foreach (var messageRecord in records)
            {
                LogEvent(messageRecord);
            }
        }
    }
}