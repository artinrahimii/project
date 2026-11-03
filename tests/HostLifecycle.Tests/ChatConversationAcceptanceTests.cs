using System.Security.Cryptography;
using WhiteP.Host.Application.Chat;
using WhiteP.Host.Windows.Chat;
using WhiteP.Host.Windows.Security;

public static class ChatConversationAcceptanceTests
{
    public static void Run()
    {
        var retentionPolicy = HostConversationRetentionPolicy.Default;
        if (retentionPolicy.Scope != HostConversationRetentionScope.ProcessSessionOnly ||
            retentionPolicy.PersistsPromptAndResponse)
            throw new Exception("The default host must not persist prompt or response content.");

        var explicitConsentPolicy = HostConversationRetentionPolicy.FromOwnerConsent(ownerConsent: true);
        if (explicitConsentPolicy.Scope != HostConversationRetentionScope.LocalDeviceWithOwnerConsent ||
            !explicitConsentPolicy.PersistsPromptAndResponse ||
            !ReferenceEquals(HostConversationRetentionPolicy.FromOwnerConsent(false), HostConversationRetentionPolicy.Default))
            throw new Exception("Durable host retention must require explicit owner consent.");

        var conversation = new ChatConversation(retentionPolicy);
        conversation.UpdateRetentionPolicy(explicitConsentPolicy);
        if (!ReferenceEquals(conversation.RetentionPolicy, explicitConsentPolicy))
            throw new Exception("The active conversation must accept an explicit owner-consented policy.");
        conversation.UpdateRetentionPolicy(retentionPolicy);
        if (!ReferenceEquals(conversation.RetentionPolicy, retentionPolicy))
            throw new Exception("ChatConversation must use the supplied host retention policy.");
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

        var separateSession = new ChatConversation();
        if (separateSession.Messages.Count != 0)
            throw new Exception("A new host session must not restore a previous session's prompt or response content.");
        if (separateSession.RetentionPolicy.PersistsPromptAndResponse)
            throw new Exception("The default host retention policy must remain session-only.");
        VerifyDpapiProtectedConversationStore();
        VerifyOrphanedTemporaryFileCleanup();
        Console.WriteLine("PASS: in-memory conversation, explicit retention consent, Windows DPAPI history and orphaned temporary-file cleanup.");
    }

    private static void VerifyOrphanedTemporaryFileCleanup()
    {
        var directory = Path.Combine(Path.GetTempPath(), "whitep-orphaned-conversation-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var historyPath = Path.Combine(directory, "history.dpapi");
        var prefix = "." + Path.GetFileName(historyPath) + ".";
        var stalePath = Path.Combine(directory, prefix + Guid.NewGuid().ToString("N") + ".tmp");
        var recentPath = Path.Combine(directory, prefix + Guid.NewGuid().ToString("N") + ".tmp");
        var activePath = Path.Combine(directory, prefix + Guid.NewGuid().ToString("N") + ".tmp");
        var malformedPath = Path.Combine(directory, prefix + "not-a-guid.tmp");
        var unrelatedPath = Path.Combine(directory, ".another-store." + Guid.NewGuid().ToString("N") + ".tmp");

        try
        {
            foreach (var path in new[] { stalePath, recentPath, activePath, malformedPath, unrelatedPath })
                File.WriteAllText(path, "non-sensitive temporary test payload");

            var oldTimestamp = DateTime.UtcNow.AddMinutes(-2);
            File.SetLastWriteTimeUtc(stalePath, oldTimestamp);
            File.SetLastWriteTimeUtc(activePath, oldTimestamp);

            using (var activeWriter = new FileStream(activePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                _ = new WindowsHostConversationStore(historyPath);
                if (File.Exists(stalePath))
                    throw new Exception("A stale, correctly named orphan temporary file was not removed at store startup.");
                if (!File.Exists(activePath))
                    throw new Exception("Startup cleanup removed a temporary file still held by an active writer.");
            }

            if (!File.Exists(recentPath) || !File.Exists(malformedPath) || !File.Exists(unrelatedPath))
                throw new Exception("Cleanup removed a recent, malformed-name or unrelated temporary file.");

            _ = new WindowsHostConversationStore(historyPath);
            if (File.Exists(activePath))
                throw new Exception("A formerly active temporary file was not removed after its writer released the handle.");

            File.SetLastWriteTimeUtc(recentPath, oldTimestamp);
            _ = new WindowsHostConversationStore(historyPath);
            if (File.Exists(recentPath))
                throw new Exception("A now-stale temporary file was not removed on a subsequent startup scan.");
            if (!File.Exists(malformedPath) || !File.Exists(unrelatedPath))
                throw new Exception("Cleanup must preserve files outside the exact store-owned GUID pattern.");
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    private static void VerifyDpapiProtectedConversationStore()
    {
        if (!OperatingSystem.IsWindows())
            return;
        var directory = Path.Combine(Path.GetTempPath(), "whitep-conversation-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "history.dpapi");
        try
        {
            var store = new WindowsHostConversationStore(path, new WindowsDpapiSecretProtector());
            var conversation = new ChatConversation(HostConversationRetentionPolicy.OwnerConsentedLocalDevice);
            const string privateMarker = "OWNER_CONSENT_PRIVATE_CONVERSATION_MARKER_8F2C";
            conversation.Add(ConversationMessageRole.User, privateMarker);
            conversation.Add(ConversationMessageRole.Assistant, "A protected local response.");

            store.Save(conversation.Messages);
            if (!File.Exists(path))
                throw new Exception("Owner-consented conversation history was not written.");
            var protectedFile = File.ReadAllBytes(path);
            var containsPlaintext = Convert.ToBase64String(protectedFile).Contains(privateMarker, StringComparison.Ordinal);
            CryptographicOperations.ZeroMemory(protectedFile);
            if (containsPlaintext)
                throw new Exception("The local history file contains plaintext prompt content.");

            var restored = store.Load();
            if (restored.Count != 2 || restored[0].Role != ConversationMessageRole.User ||
                restored[0].Content != privateMarker || restored[1].Role != ConversationMessageRole.Assistant ||
                restored[1].Content != "A protected local response.")
                throw new Exception("DPAPI-protected local history did not round-trip the conversation.");

            var corruptBytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            File.WriteAllBytes(path, corruptBytes);
            var rejectedCorruptFile = false;
            try { _ = store.Load(); }
            catch (InvalidDataException) { rejectedCorruptFile = true; }
            if (!rejectedCorruptFile || !File.ReadAllBytes(path).SequenceEqual(corruptBytes))
                throw new Exception("Invalid protected history must fail closed without overwriting the file.");
            store.Delete();
            if (File.Exists(path))
                throw new Exception("Clearing local history must remove the protected file.");
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }
}
