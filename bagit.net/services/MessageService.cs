using bagit.net.domain;

public class MessageService : IMessageService
{
    private readonly List<MessageRecord> _messages = new();
    public void Add(MessageRecord message) => _messages.Add(message);
    public void AddRange(IEnumerable<MessageRecord> messages) => _messages.AddRange(messages);
    public IReadOnlyList<MessageRecord> GetAll() => _messages.AsReadOnly();
    public void Clear() => _messages.Clear();
}