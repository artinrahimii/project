using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using WhiteP.Host.Application.Chat;
using WhiteP.Host.Windows.Security;
using WhiteP.Shared.Security;

namespace WhiteP.Host.Windows.Chat;

/// <summary>
/// Stores optional host conversation history as a DPAPI-protected payload for
/// the current Windows user. Callers must obtain explicit owner consent before use.
/// </summary>
public sealed class WindowsHostConversationStore
{
    private const int CurrentSchemaVersion = 1;
    private const int MaximumPlaintextBytes = 4 * 1024 * 1024;
    private const int MaximumProtectedBytes = MaximumPlaintextBytes + (128 * 1024);
    private const int MaximumMessages = 2_000;
    private const int MaximumMessageCharacters = 256 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _filePath;
    private readonly IUserSecretProtector _protector;

    public WindowsHostConversationStore(string? filePath = null, IUserSecretProtector? protector = null)
    {
        _filePath = Path.GetFullPath(filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WhiteP",
            "host-conversation.dpapi"));
        _protector = protector ?? new WindowsDpapiSecretProtector();
    }

    public IReadOnlyList<ConversationMessage> Load()
    {
        if (!File.Exists(_filePath))
            return Array.Empty<ConversationMessage>();
        var fileInfo = new FileInfo(_filePath);
        if (fileInfo.Length is <= 0 or > MaximumProtectedBytes)
            throw new InvalidDataException("The protected host conversation file has an invalid size.");

        byte[] protectedPayload = File.ReadAllBytes(_filePath);
        byte[] plaintext = Array.Empty<byte>();
        try
        {
            if (protectedPayload.Length is <= 0 or > MaximumProtectedBytes)
                throw new InvalidDataException("The protected host conversation file changed while being read.");
            try
            {
                plaintext = _protector.Unprotect(protectedPayload);
            }
            catch (CryptographicException exception)
            {
                throw new InvalidDataException("The protected host conversation could not be decrypted for this Windows user.", exception);
            }
            if (plaintext.Length is <= 0 or > MaximumPlaintextBytes)
                throw new InvalidDataException("The unprotected host conversation has an invalid size.");

            ConversationDocument document;
            try
            {
                document = JsonSerializer.Deserialize<ConversationDocument>(plaintext, JsonOptions)
                    ?? throw new InvalidDataException("The host conversation document is empty.");
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException("The unprotected host conversation document is malformed.", exception);
            }
            if (document.SchemaVersion != CurrentSchemaVersion || document.Messages is null)
                throw new InvalidDataException("The host conversation document version or message collection is invalid.");
            ValidateMessages(document.Messages);
            return document.Messages;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(protectedPayload);
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    public void Save(IReadOnlyList<ConversationMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ValidateMessages(messages);
        byte[] plaintext = JsonSerializer.SerializeToUtf8Bytes(
            new ConversationDocument(CurrentSchemaVersion, messages.ToArray()), JsonOptions);
        byte[] protectedPayload = Array.Empty<byte>();
        string? temporaryPath = null;
        try
        {
            if (plaintext.Length is <= 0 or > MaximumPlaintextBytes)
                throw new InvalidDataException("The host conversation exceeds the permitted local-history size.");
            protectedPayload = _protector.Protect(plaintext);
            if (protectedPayload.Length is <= 0 or > MaximumProtectedBytes)
                throw new InvalidDataException("The protected host conversation exceeds the permitted file size.");

            var directory = Path.GetDirectoryName(_filePath)
                ?? throw new InvalidOperationException("The host conversation path must include a directory.");
            Directory.CreateDirectory(directory);
            temporaryPath = Path.Combine(directory, $".{Path.GetFileName(_filePath)}.{Guid.NewGuid():N}.tmp");
            using (var stream = new FileStream(
                temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                bufferSize: 4096, options: FileOptions.WriteThrough))
            {
                stream.Write(protectedPayload);
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporaryPath, _filePath, overwrite: true);
            temporaryPath = null;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
            CryptographicOperations.ZeroMemory(protectedPayload);
            if (temporaryPath is not null && File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    public void Delete()
    {
        if (File.Exists(_filePath))
            File.Delete(_filePath);
    }

    private static void ValidateMessages(IReadOnlyList<ConversationMessage> messages)
    {
        if (messages.Count > MaximumMessages)
            throw new InvalidDataException("The host conversation contains too many messages.");
        for (var index = 0; index < messages.Count; index++)
        {
            var message = messages[index];
            if (message is null || message.Sequence != index + 1 ||
                !Enum.IsDefined(message.Role) || string.IsNullOrWhiteSpace(message.Content) ||
                message.Content.Length > MaximumMessageCharacters)
                throw new InvalidDataException("The host conversation contains an invalid message.");
        }
    }

    private sealed record ConversationDocument(int SchemaVersion, ConversationMessage[] Messages);
}
