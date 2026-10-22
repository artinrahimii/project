using Google.Protobuf;
using System.Linq;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.V1;

namespace WhiteP.Host.Windows;

public readonly record struct ProtocolMessageParseResult<T>(
    T? Message,
    ProtocolMessageValidationResult Validation)
    where T : class
{
    public bool IsValid => Message is not null && Validation.IsValid;
}

/// <summary>
/// Size-checks raw bytes before Protobuf parsing, then validates parsed field values.
/// A future transport must call this boundary before dispatching messages.
/// </summary>
public static class ProtocolMessageParser
{
    public static ProtocolMessageParseResult<InferenceRequest> ParseInferenceRequest(byte[]? wireBytes)
    {
        if (wireBytes is null)
        {
            return Invalid<InferenceRequest>(ProtocolMessageValidator.InvalidMessage(
                "InferenceRequest wire bytes are missing."));
        }

        var sizeResult = ProtocolMessageValidator.FromLimitResult(
            ProtocolMessageLimits.ValidateRequestMessageBytes(wireBytes.LongLength));
        if (!sizeResult.IsValid)
        {
            return Invalid<InferenceRequest>(sizeResult);
        }

        try
        {
            var request = InferenceRequest.Parser.ParseFrom(wireBytes);
            var validation = ProtocolMessageValidator.ValidateInferenceRequest(
                request.RequestId,
                request.AttemptId,
                request.ModelId,
                request.Prompt,
                request.DeadlineUnixMs,
                request.MaxOutputTokens,
                request.ContextCapsule.Length,
                request.CalculateSize());

            return validation.IsValid
                ? new ProtocolMessageParseResult<InferenceRequest>(request, validation)
                : Invalid<InferenceRequest>(validation);
        }
        catch (InvalidProtocolBufferException)
        {
            return Invalid<InferenceRequest>(ProtocolMessageValidator.InvalidMessage(
                "InferenceRequest contains malformed Protobuf wire data."));
        }
    }

    public static ProtocolMessageParseResult<HostAnnouncement> ParseHostAnnouncement(byte[]? wireBytes)
    {
        if (wireBytes is null)
        {
            return Invalid<HostAnnouncement>(ProtocolMessageValidator.InvalidMessage(
                "HostAnnouncement wire bytes are missing."));
        }

        var sizeResult = ProtocolMessageValidator.FromLimitResult(
            ProtocolMessageLimits.ValidateControlMessageBytes(wireBytes.LongLength));
        if (!sizeResult.IsValid)
        {
            return Invalid<HostAnnouncement>(sizeResult);
        }

        try
        {
            var announcement = HostAnnouncement.Parser.ParseFrom(wireBytes);
            var capabilities = announcement.Capabilities;
            if (capabilities is null)
            {
                return Invalid<HostAnnouncement>(ProtocolMessageValidator.InvalidMessage(
                    "HostAnnouncement is missing HostCapabilities."));
            }

            var models = capabilities.Models
                .Select(model => new ModelCapabilityValidationInput(
                    model.ModelId,
                    model.DisplayName,
                    model.MaxContextTokens,
                    model.MaxOutputTokens))
                .ToArray();

            var validation = ProtocolMessageValidator.ValidateHostAnnouncement(
                announcement.EphemeralHostId,
                announcement.ProtocolVersion,
                capabilities.EphemeralHostId,
                capabilities.ProtocolVersion,
                announcement.AnnouncedAtUnixMs,
                capabilities.MaxConcurrentRequests,
                capabilities.MaxContextCapsuleBytes,
                capabilities.MaxPromptBytes,
                models);

            return validation.IsValid
                ? new ProtocolMessageParseResult<HostAnnouncement>(announcement, validation)
                : Invalid<HostAnnouncement>(validation);
        }
        catch (InvalidProtocolBufferException)
        {
            return Invalid<HostAnnouncement>(ProtocolMessageValidator.InvalidMessage(
                "HostAnnouncement contains malformed Protobuf wire data."));
        }
    }

    private static ProtocolMessageParseResult<T> Invalid<T>(ProtocolMessageValidationResult validation)
        where T : class => new(null, validation);
}
