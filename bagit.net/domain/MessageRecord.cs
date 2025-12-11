namespace bagit.net.domain
{
    public enum MessageLevel
    {
        DEBUG,
        WARNING,
        ERROR,
        INFO,
        NULL,
    }

    public record MessageRecord
    {
        readonly string _message;
        readonly MessageLevel _messageLevel;

        public MessageRecord(MessageLevel messageLevel, string message)
        {
            _messageLevel = messageLevel;
            _message = message;
        }

        public override string ToString()
        {
            return $"[{_messageLevel}] {_message}";
        }

        public string GetMessage() { return _message; }
        public MessageLevel GetLevel() { return _messageLevel; }
    }


    public static class MessageHelpers
    {
        public static bool HasError(IEnumerable<MessageRecord> messages)
        {
            foreach (var record in messages) {
                if (record.GetLevel() == MessageLevel.ERROR) {
                    return true;
                }
            }
            return false;
        }

        public static bool HasWarning(IEnumerable<MessageRecord> messages)
        {
            foreach (var record in messages)
            {
                if (record.GetLevel() == MessageLevel.WARNING)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static class MessageContext
    {
        public static readonly AsyncLocal<bool> Quiet = new();
    }
}

