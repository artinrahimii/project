using System;
using System.Collections.Generic;

namespace WhiteP.Shared.Contracts;

/// <summary>Classifies validation failures before a parsed message may reach inference or application logic.</summary>
public enum ProtocolMessageValidationKind
{
    Valid = 0,
    InvalidMessage = 1,
    UnsupportedVersion = 2,
    ResourceLimit = 3,
    RequestExpired = 4
}

public readonly record struct ProtocolMessageValidationResult(
    ProtocolMessageValidationKind Kind,
    string? ErrorCodeName,
    string Message)
{
    public bool IsValid => Kind == ProtocolMessageValidationKind.Valid;
}

public readonly record struct ModelCapabilityValidationInput(
    string? ModelId,
    string? DisplayName,
    ulong MaxContextTokens,
    ulong MaxOutputTokens);

/// <summary>
/// Shared semantic checks for values in generated Protobuf messages. Call this after parsing;
/// callers handling raw bytes must apply the byte ceiling before parsing.
/// </summary>
public static class ProtocolMessageValidator
{
    public const string RequestExpiredErrorCode = "PROTOCOL_ERROR_CODE_REQUEST_EXPIRED";
    private const int MaximumRequestIdentifierLength = 128;
    public static ProtocolMessageValidationResult ValidateInferenceRequest(
        string? protocolVersion,
        string? requestId,
        string? attemptId,
        string? modelId,
        string? prompt,
        ulong deadlineUnixMs,
        uint maxOutputTokens,
        long contextCapsuleBytes,
        long serializedMessageBytes,
        DateTimeOffset? nowUtc = null)
    {
        if (!IsValidRequestIdentifier(requestId) ||
            !IsValidRequestIdentifier(attemptId) ||
            string.IsNullOrWhiteSpace(modelId) ||
            string.IsNullOrWhiteSpace(prompt))
        {
            return InvalidMessage("InferenceRequest is missing or has an invalid request/attempt identifier, model, or prompt.");
        }

        if (string.IsNullOrWhiteSpace(protocolVersion))
        {
            return InvalidMessage("InferenceRequest must include protocol_version.");
        }

        var versionResult = FromVersionResult(ProtocolVersionPolicy.Evaluate(protocolVersion));
        if (!versionResult.IsValid)
        {
            return versionResult;
        }

        var nowUnixMs = (nowUtc ?? DateTimeOffset.UtcNow).ToUnixTimeMilliseconds();
        var maximumDeadlineUnixMs = (ulong)DateTimeOffset.MaxValue.ToUnixTimeMilliseconds();
        if (deadlineUnixMs == 0 ||
            deadlineUnixMs > maximumDeadlineUnixMs ||
            nowUnixMs < 0)
        {
            return InvalidMessage("InferenceRequest must include a valid Unix-millisecond deadline.");
        }

        if (deadlineUnixMs <= (ulong)nowUnixMs)
        {
            return new ProtocolMessageValidationResult(
                ProtocolMessageValidationKind.RequestExpired,
                RequestExpiredErrorCode,
                "InferenceRequest deadline has expired.");
        }

        if (maxOutputTokens == 0)
        {
            return InvalidMessage("InferenceRequest max_output_tokens must be greater than zero.");
        }

        return FromLimitResult(ProtocolMessageLimits.ValidateInferenceRequest(
            prompt,
            contextCapsuleBytes,
            serializedMessageBytes,
            maxOutputTokens));
    }

    private static bool IsValidRequestIdentifier(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > MaximumRequestIdentifierLength)
        {
            return false;
        }

        if (!IsAsciiAlphaNumeric(value[0]))
        {
            return false;
        }

        foreach (var character in value)
        {
            if (!IsAsciiAlphaNumeric(character) &&
                character != '_' &&
                character != '-' &&
                character != '.' &&
                character != ':')
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsAsciiAlphaNumeric(char value) =>
        (value >= 'a' && value <= 'z') ||
        (value >= 'A' && value <= 'Z') ||
        (value >= '0' && value <= '9');

    public static ProtocolMessageValidationResult ValidateHostAnnouncement(
        string? ephemeralHostId,
        string? protocolVersion,
        string? capabilitiesHostId,
        string? capabilitiesProtocolVersion,
        ulong announcedAtUnixMs,
        uint maxConcurrentRequests,
        ulong maxContextCapsuleBytes,
        ulong maxPromptBytes,
        IReadOnlyCollection<ModelCapabilityValidationInput>? models)
    {
        if (string.IsNullOrWhiteSpace(ephemeralHostId) ||
            string.IsNullOrWhiteSpace(capabilitiesHostId) ||
            string.IsNullOrWhiteSpace(protocolVersion) ||
            string.IsNullOrWhiteSpace(capabilitiesProtocolVersion))
        {
            return InvalidMessage("HostAnnouncement is missing required host identity, version, or capabilities.");
        }

        var versionResult = FromVersionResult(ProtocolVersionPolicy.Evaluate(protocolVersion));
        if (!versionResult.IsValid)
        {
            return versionResult;
        }

        var capabilitiesVersionResult = FromVersionResult(ProtocolVersionPolicy.Evaluate(capabilitiesProtocolVersion));
        if (!capabilitiesVersionResult.IsValid)
        {
            return capabilitiesVersionResult;
        }

        if (!string.Equals(ephemeralHostId, capabilitiesHostId, StringComparison.Ordinal))
        {
            return InvalidMessage("HostAnnouncement and HostCapabilities host identifiers must match.");
        }

        if (!string.Equals(protocolVersion, capabilitiesProtocolVersion, StringComparison.Ordinal))
        {
            return InvalidMessage("HostAnnouncement and HostCapabilities protocol versions must match.");
        }

        if (announcedAtUnixMs == 0 || maxConcurrentRequests != 1)
        {
            return InvalidMessage("HostAnnouncement has an invalid timestamp or concurrency value.");
        }

        if (maxContextCapsuleBytes == 0 || maxPromptBytes == 0)
        {
            return InvalidMessage("HostCapabilities must advertise positive prompt and capsule limits.");
        }

        if (maxContextCapsuleBytes > (ulong)ProtocolMessageLimits.MaximumContextCapsuleBytes ||
            maxPromptBytes > (ulong)ProtocolMessageLimits.MaximumPromptUtf8Bytes)
        {
            return ResourceLimit("HostCapabilities advertises payload limits above the local protocol ceilings.");
        }

        if (models is null || models.Count == 0)
        {
            return InvalidMessage("HostCapabilities must advertise at least one model.");
        }

        foreach (var model in models)
        {
            if (string.IsNullOrWhiteSpace(model.ModelId) ||
                string.IsNullOrWhiteSpace(model.DisplayName) ||
                model.MaxContextTokens == 0 ||
                model.MaxOutputTokens == 0)
            {
                return InvalidMessage("HostCapabilities contains a model with missing or invalid values.");
            }

            if (model.MaxOutputTokens > ProtocolMessageLimits.MaximumOutputTokens)
            {
                return ResourceLimit("HostCapabilities advertises a model output-token limit above the local ceiling.");
            }
        }

        return Valid();
    }

    public static ProtocolMessageValidationResult ValidateInferenceResponse(
        string? requestId,
        string? attemptId,
        string? ephemeralHostId,
        int outcomeValue,
        string? outputText,
        ulong completedAtUnixMs,
        string? modelId,
        string? errorMessage,
        long serializedMessageBytes)
    {
        if (string.IsNullOrWhiteSpace(requestId) ||
            string.IsNullOrWhiteSpace(attemptId) ||
            string.IsNullOrWhiteSpace(ephemeralHostId) ||
            string.IsNullOrWhiteSpace(modelId) ||
            completedAtUnixMs == 0)
        {
            return InvalidMessage("InferenceResponse is missing a required identifier or completion timestamp.");
        }

        if (outcomeValue < 1 || outcomeValue > 5)
        {
            return InvalidMessage("InferenceResponse outcome is unspecified or contains an unknown enum value.");
        }

        var limitsResult = FromLimitResult(ProtocolMessageLimits.ValidateInferenceResponse(
            outputText,
            serializedMessageBytes));
        if (!limitsResult.IsValid)
        {
            return limitsResult;
        }

        if (outcomeValue == 1 && string.IsNullOrWhiteSpace(outputText))
        {
            return InvalidMessage("A successful InferenceResponse must contain generated output text.");
        }

        if (outcomeValue == 1 && !string.IsNullOrWhiteSpace(errorMessage))
        {
            return InvalidMessage("A successful InferenceResponse must not contain an error message.");
        }

        return Valid();
    }

    public static ProtocolMessageValidationResult FromLimitResult(ProtocolLimitValidationResult result)
    {
        if (result.IsValid)
        {
            return Valid();
        }

        if (result.Violation == ProtocolLimitViolation.InvalidLength)
        {
            return InvalidMessage(result.Message);
        }

        return ResourceLimit(result.Message);
    }

    public static ProtocolMessageValidationResult FromVersionResult(ProtocolVersionValidationResult result) =>
        result.Status switch
        {
            ProtocolVersionStatus.Compatible => Valid(),
            ProtocolVersionStatus.Unsupported => new(
                ProtocolMessageValidationKind.UnsupportedVersion,
                ProtocolVersionPolicy.UnsupportedVersionErrorCode,
                result.Message),
            _ => InvalidMessage(result.Message)
        };

    public static ProtocolMessageValidationResult InvalidMessage(string message) =>
        new(ProtocolMessageValidationKind.InvalidMessage, ProtocolVersionPolicy.InvalidMessageErrorCode, message);

    public static ProtocolMessageValidationResult ResourceLimit(string message) =>
        new(ProtocolMessageValidationKind.ResourceLimit, ProtocolMessageLimits.ResourceLimitErrorCode, message);

    private static ProtocolMessageValidationResult Valid() =>
        new(ProtocolMessageValidationKind.Valid, null, "Message is valid.");
}
