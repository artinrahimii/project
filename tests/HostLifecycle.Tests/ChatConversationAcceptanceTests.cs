using WhiteP.Host.Application.Chat;

public static class ChatConversationAcceptanceTests
{
    public static void Run()
    {
        var conversation = new ChatConversation();
        var user = conversation.Add(ConversationMessageRole.User, "Hello");
        var assistant = conversation.Add(ConversationMessageRole.Assistant, "Hello from the local model");
        var system = conversation.Add(ConversationMessageRole.System, "Inference cancelled");

        if (user.Sequence != 1 || assistant.Sequence != 2 || system.Sequence != 3)
            throw new Exception("Conversation sequence must increase deterministically.");
        if (conversation.Messages.Count != 3 || conversation.Messages[0] != user ||
            conversation.Messages[1] != assistant || conversation.Messages[2] != system)
            throw new Exception("Conversation must preserve insertion order and message roles.");
        if (conversation.Messages[0].Content != "Hello" ||
            conversation.Messages[1].Content != "Hello from the local model" ||
            conversation.Messages[2].Content != "Inference cancelled")
            throw new Exception("Conversation must retain the exact message content.");

        var rejectedEmptyMessage = false;
        try { conversation.Add(ConversationMessageRole.User, "  "); }
        catch (ArgumentException) { rejectedEmptyMessage = true; }
        if (!rejectedEmptyMessage)
            throw new Exception("Whitespace-only conversation messages must be rejected.");
        if (conversation.Messages.Count != 3)
            throw new Exception("Rejected messages must not mutate the conversation.");

        conversation.Clear();
        if (conversation.Messages.Count != 0)
            throw new Exception("Clearing a session must remove all in-memory messages.");
        var afterClear = conversation.Add(ConversationMessageRole.User, "New session");
        if (afterClear.Sequence != 1 || conversation.Messages.Count != 1)
            throw new Exception("A cleared session must restart its sequence at one.");
        Console.WriteLine("PASS: in-memory conversation preserves deterministic order, roles, and content.");
    }
}
