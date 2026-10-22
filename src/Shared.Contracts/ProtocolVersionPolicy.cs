namespace WhiteP.Shared.Contracts;

/// <summary>
/// Result of evaluating a peer's advertised protocol version.
/// ErrorCodeName is the canonical Protobuf enum constant name for the future transport adapter.
/// </summary>
public enum ProtocolVersionStatus
{
    Compatible = 0,
    Missing = 1,
    Malformed = 2,
    Unsupported = 3
}

public readonly record struct ProtocolVersionValidationResult(
    ProtocolVersionStatus Status,
    string? ErrorCodeName,
    string Message)
{
    public bool IsCompatible => Status == ProtocolVersionStatus.Compatible;
}

/// <summary>
/// Conservative version gate for the initial wire protocol.
/// Exact matching is intentional until backward-compatible minor-version negotiation is specified.
/// A transport must evaluate the remote announcement/capabilities before processing requests.
/// </summary>
public static class ProtocolVersionPolicy
{
    public const string CurrentVersion = "1.0.0";
    public const string InvalidMessageErrorCode = "PROTOCOL_ERROR_CODE_INVALID_MESSAGE";
    public const string UnsupportedVersionErrorCode = "PROTOCOL_ERROR_CODE_UNSUPPORTED_VERSION";

    private const int MaximumVersionTextLength = 32;

    public static ProtocolVersionValidationResult Evaluate(string? remoteVersion)
    {
        if (string.IsNullOrWhiteSpace(remoteVersion))
        {
            return new ProtocolVersionValidationResult(
                ProtocolVersionStatus.Missing,
                InvalidMessageErrorCode,
                "The peer did not advertise a protocol version.");
        }

        if (!IsCanonicalNumericVersion(remoteVersion))
        {
            return new ProtocolVersionValidationResult(
                ProtocolVersionStatus.Malformed,
                InvalidMessageErrorCode,
                "The peer protocol version must use canonical MAJOR.MINOR.PATCH notation.");
        }

        if (!string.Equals(remoteVersion, CurrentVersion, StringComparison.Ordinal))
        {
            return new ProtocolVersionValidationResult(
                ProtocolVersionStatus.Unsupported,
                UnsupportedVersionErrorCode,
                "The peer protocol version is not supported.");
        }

        return new ProtocolVersionValidationResult(
            ProtocolVersionStatus.Compatible,
            null,
            "The peer protocol version is supported.");
    }

    private static bool IsCanonicalNumericVersion(string value)
    {
        if (value.Length > MaximumVersionTextLength)
        {
            return false;
        }

        var components = value.Split('.', StringSplitOptions.None);
        if (components.Length != 3)
        {
            return false;
        }

        foreach (var component in components)
        {
            if (component.Length == 0 ||
                (component.Length > 1 && component[0] == '0'))
            {
                return false;
            }

            foreach (var character in component)
            {
                if (character < '0' || character > '9')
                {
                    return false;
                }
            }
        }

        return true;
    }
}
