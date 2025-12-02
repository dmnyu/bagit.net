using System.Reflection.Emit;
using static System.Net.Mime.MediaTypeNames;

namespace bagit.net.domain
{
    public enum MessageLevel
    {
        DEBUG,
        WARNING,
        ERROR,
        INFO,
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

        public string GetMessage() { return _message ; }
        public MessageLevel GetLevel() { return _messageLevel ; }
    }
}
