namespace WhiteP.Host.Application.Chat;

public enum ConversationMessageRole
{
    User,
    Assistant,
    System
}

public sealed record ConversationMessage(long Sequence, ConversationMessageRole Role, string Content);

/// <summary>Maintains an ordered, in-memory conversation for the lifetime of a host UI session.</summary>
public sealed class ChatConversation
{
    private readonly List<ConversationMessage> _messages = [];
    private long _nextSequence = 1;

    public IReadOnlyList<ConversationMessage> Messages => _messages.AsReadOnly();

    public void Clear()
    {
        _messages.Clear();
        _nextSequence = 1;
    }

    public ConversationMessage Add(ConversationMessageRole role, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        var message = new ConversationMessage(_nextSequence++, role, content);
        _messages.Add(message);
        return message;
    }
}
