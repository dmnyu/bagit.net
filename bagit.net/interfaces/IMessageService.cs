using bagit.net.domain;

public interface IMessageService
{
    void Add(MessageRecord message);
    void AddRange(IEnumerable<MessageRecord> records);
    IReadOnlyList<MessageRecord> GetAll();
    void Clear();
}