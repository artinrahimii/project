using System.Text;

namespace WhiteP.Shared.Contracts;

/// <summary>
/// Initial hard ceilings for protocol payload sizes. UTF-8 counts measure encoded bytes,
/// not UTF-16 characters. A transport must check raw frame length before parsing and then
/// validate the typed message with these same limits.
/// </summary>
public static class ProtocolMessageLimits
{
    public const int MaximumRequestMessageBytes = 16 * 1024;
    public const int MaximumPromptUtf8Bytes = 8 * 1024;
    public const int MaximumContextCapsuleBytes = 4 * 1024;
    public const uint MaximumOutputTokens = 131_072U;
    public const int MaximumResponseTextUtf8Bytes = 1024 * 1024;
    public const int MaximumResponseMessageBytes = 2 * 1024 * 1024;

    // The local llama.cpp adapter uses JSON rather than the future peer wire framing.
    public const int MaximumLocalRequestBodyBytes = 64 * 1024;
    public const int MaximumLocalResponseBodyBytes = 2 * 1024 * 1024;

    public const string ResourceLimitErrorCode = "PROTOCOL_ERROR_CODE_RESOURCE_LIMIT";

    public static ProtocolLimitValidationResult ValidateInferenceRequest(
        string? prompt,
        long contextCapsuleBytes,
        long serializedMessageBytes,
        uint maxOutputTokens)
    {
        var promptResult = ValidatePromptText(prompt);
        if (!promptResult.IsValid)
        {
            return promptResult;
        }

        var capsuleResult = ValidateContextCapsuleBytes(contextCapsuleBytes);
        if (!capsuleResult.IsValid)
        {
            return capsuleResult;
        }

        var messageResult = ValidateRequestMessageBytes(serializedMessageBytes);
        if (!messageResult.IsValid)
        {
            return messageResult;
        }

        if (maxOutputTokens > MaximumOutputTokens)
        {
            return Invalid(
                ProtocolLimitViolation.OutputTokenLimitExceeded,
                $"Maximum output tokens exceeds the protocol limit of {MaximumOutputTokens}.");
        }

        return Valid();
    }

    public static ProtocolLimitValidationResult ValidatePromptText(string? prompt)
    {
        var byteCount = prompt is null ? 0 : Encoding.UTF8.GetByteCount(prompt);
        return ValidateSize(
            byteCount,
            MaximumPromptUtf8Bytes,
            ProtocolLimitViolation.PromptTooLarge,
            $"Prompt exceeds the protocol limit of {MaximumPromptUtf8Bytes} UTF-8 bytes.");
    }

    public static ProtocolLimitValidationResult ValidateContextCapsuleBytes(long byteCount) =>
        ValidateSize(
            byteCount,
            MaximumContextCapsuleBytes,
            ProtocolLimitViolation.ContextCapsuleTooLarge,
            $"Context capsule exceeds the protocol limit of {MaximumContextCapsuleBytes} bytes.");

    public static ProtocolLimitValidationResult ValidateRequestMessageBytes(long byteCount) =>
        ValidateSize(
            byteCount,
            MaximumRequestMessageBytes,
            ProtocolLimitViolation.RequestMessageTooLarge,
            $"Serialized inference request exceeds the protocol limit of {MaximumRequestMessageBytes} bytes.");

    public static ProtocolLimitValidationResult ValidateResponseText(string? outputText)
    {
        var byteCount = outputText is null ? 0 : Encoding.UTF8.GetByteCount(outputText);
        return ValidateSize(
            byteCount,
            MaximumResponseTextUtf8Bytes,
            ProtocolLimitViolation.ResponseTextTooLarge,
            $"Generated response text exceeds the protocol limit of {MaximumResponseTextUtf8Bytes} UTF-8 bytes.");
    }

    public static ProtocolLimitValidationResult ValidateResponseMessageBytes(long byteCount) =>
        ValidateSize(
            byteCount,
            MaximumResponseMessageBytes,
            ProtocolLimitViolation.ResponseMessageTooLarge,
            $"Serialized inference response exceeds the protocol limit of {MaximumResponseMessageBytes} bytes.");

    public static ProtocolLimitValidationResult ValidateLocalRequestBodyBytes(long byteCount) =>
        ValidateSize(
            byteCount,
            MaximumLocalRequestBodyBytes,
            ProtocolLimitViolation.LocalRequestBodyTooLarge,
            $"Local inference request body exceeds the configured limit of {MaximumLocalRequestBodyBytes} bytes.");

    public static ProtocolLimitValidationResult ValidateLocalResponseBodyBytes(long byteCount) =>
        ValidateSize(
            byteCount,
            MaximumLocalResponseBodyBytes,
            ProtocolLimitViolation.LocalResponseBodyTooLarge,
            $"Local inference response body exceeds the configured limit of {MaximumLocalResponseBodyBytes} bytes.");

    public static ProtocolLimitValidationResult ValidateInferenceResponse(
        string? outputText,
        long serializedMessageBytes)
    {
        var textResult = ValidateResponseText(outputText);
        if (!textResult.IsValid)
        {
            return textResult;
        }

        return ValidateResponseMessageBytes(serializedMessageBytes);
    }

    private static ProtocolLimitValidationResult ValidateSize(
        long actualBytes,
        long maximumBytes,
        ProtocolLimitViolation overLimit,
        string message)
    {
        if (actualBytes < 0)
        {
            return Invalid(ProtocolLimitViolation.InvalidLength, "Payload byte length cannot be negative.");
        }

        return actualBytes > maximumBytes
            ? Invalid(overLimit, message)
            : Valid();
    }

    private static ProtocolLimitValidationResult Valid() =>
        new(ProtocolLimitViolation.None, string.Empty);

    private static ProtocolLimitValidationResult Invalid(
        ProtocolLimitViolation violation,
        string message) => new(violation, message);
}

public enum ProtocolLimitViolation
{
    None = 0,
    InvalidLength = 1,
    RequestMessageTooLarge = 2,
    PromptTooLarge = 3,
    ContextCapsuleTooLarge = 4,
    OutputTokenLimitExceeded = 5,
    ResponseMessageTooLarge = 6,
    ResponseTextTooLarge = 7,
    LocalRequestBodyTooLarge = 8,
    LocalResponseBodyTooLarge = 9
}

public readonly record struct ProtocolLimitValidationResult(
    ProtocolLimitViolation Violation,
    string Message)
{
    public bool IsValid => Violation == ProtocolLimitViolation.None;

    public string? ErrorCodeName => Violation switch
    {
        ProtocolLimitViolation.None => null,
        ProtocolLimitViolation.InvalidLength => ProtocolVersionPolicy.InvalidMessageErrorCode,
        _ => ProtocolMessageLimits.ResourceLimitErrorCode
    };
}
