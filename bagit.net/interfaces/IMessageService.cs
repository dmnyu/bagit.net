using bagit.net.domain;

namespace bagit.net.interfaces
{
    public interface IMessageService
    {
        void Add(MessageRecord message);
        void AddRange(IEnumerable<MessageRecord> records);
        IReadOnlyList<MessageRecord> GetAll();
        void Clear();
        public void LogEvent(MessageRecord messageRecord);
        public void LogEvents(IEnumerable<MessageRecord> messageRecords);
    }
}