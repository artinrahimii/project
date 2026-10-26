using System;
using Google.Protobuf;
using WhiteP.Host.Windows.Requests;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.V1;

namespace WhiteP.Host.Windows.Networking;

/// <summary>
/// Converts host admission/validation failures into stable protocol message types.
/// This mapper does not serialize or send a TCP response; that is a separate transport step.
/// </summary>
public static class NetworkInferenceRequestResponseMapper
{
    private const long BusyRetryDelayMilliseconds = 1_000;

    public static IMessage MapAdmissionOutcome<T>(
        HostRequestOutcome<T> outcome,
        string requestId,
        string requestedModelId,
        string ephemeralHostId,
        DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        RequireIdentifier(requestId, nameof(requestId));

        return outcome.Status switch
        {
            HostRequestStatus.Busy => CreateBusy(requestId, ephemeralHostId, nowUtc),
            HostRequestStatus.Rejected => MapAdmissionRejection(
                requestId,
                requestedModelId,
                ephemeralHostId,
                outcome.AdmissionFailure ?? HostRequestAdmissionFailure.InvalidDeadline,
                nowUtc),
            HostRequestStatus.Completed => throw new ArgumentException(
                "A completed request does not map to a failure response.",
                nameof(outcome)),
            _ => CreateInternalError(requestId)
        };
    }

    public static ProtocolError CreateInvalidRequest(
        string? requestId,
        ProtocolMessageValidationResult validation)
    {
        if (validation.IsValid)
        {
            throw new ArgumentException("A valid message cannot be mapped to an invalid-request response.", nameof(validation));
        }

        var code = validation.ErrorCodeName switch
        {
            ProtocolVersionPolicy.UnsupportedVersionErrorCode => ProtocolErrorCode.UnsupportedVersion,
            ProtocolMessageValidator.RequestExpiredErrorCode => ProtocolErrorCode.RequestExpired,
            ProtocolMessageLimits.ResourceLimitErrorCode => ProtocolErrorCode.ResourceLimit,
            _ => ProtocolErrorCode.InvalidMessage
        };

        var message = code switch
        {
            ProtocolErrorCode.UnsupportedVersion => "The request protocol version is not supported.",
            ProtocolErrorCode.RequestExpired => "The request deadline has expired.",
            ProtocolErrorCode.ResourceLimit => "The request exceeds the protocol resource limits.",
            _ => "The request is invalid."
        };

        return CreateProtocolError(requestId, code, message, retryable: false);
    }

    public static ProtocolError CreateInternalError(string? requestId) =>
        CreateProtocolError(
            requestId,
            ProtocolErrorCode.Internal,
            "The host could not process the request.",
            retryable: false);

    private static IMessage MapAdmissionRejection(
        string requestId,
        string requestedModelId,
        string ephemeralHostId,
        HostRequestAdmissionFailure failure,
        DateTimeOffset nowUtc)
    {
        return failure switch
        {
            HostRequestAdmissionFailure.ModelMismatch => CreateUnsupportedModel(requestId, requestedModelId),
            HostRequestAdmissionFailure.InvalidDeadline => CreateProtocolError(
                requestId,
                ProtocolErrorCode.InvalidMessage,
                "The request deadline is missing or invalid.",
                retryable: false),
            HostRequestAdmissionFailure.DeadlineExpired => CreateProtocolError(
                requestId,
                ProtocolErrorCode.RequestExpired,
                "The request deadline has expired.",
                retryable: false),
            HostRequestAdmissionFailure.CapacityUnknown or HostRequestAdmissionFailure.CapacityInsufficient =>
                CreateProtocolError(
                    requestId,
                    ProtocolErrorCode.ResourceLimit,
                    "The host cannot currently admit this request because capacity is unavailable.",
                    retryable: false),
            HostRequestAdmissionFailure.InactiveParticipation or HostRequestAdmissionFailure.ModelNotReady =>
                CreateBusy(requestId, ephemeralHostId, nowUtc),
            _ => CreateProtocolError(
                requestId,
                ProtocolErrorCode.InvalidMessage,
                "The request was rejected by the admission policy.",
                retryable: false)
        };
    }

    private static HostBusy CreateBusy(string requestId, string ephemeralHostId, DateTimeOffset nowUtc)
    {
        RequireIdentifier(ephemeralHostId, nameof(ephemeralHostId));
        var nowUnixMs = Math.Max(0L, nowUtc.ToUnixTimeMilliseconds());
        var maximumUnixMs = DateTimeOffset.MaxValue.ToUnixTimeMilliseconds();
        var retryAfterUnixMs = nowUnixMs >= maximumUnixMs - BusyRetryDelayMilliseconds
            ? maximumUnixMs
            : nowUnixMs + BusyRetryDelayMilliseconds;

        return new HostBusy
        {
            RequestId = requestId,
            EphemeralHostId = ephemeralHostId,
            RetryAfterUnixMs = (ulong)retryAfterUnixMs,
            Message = "The host is busy or temporarily unavailable."
        };
    }

    private static UnsupportedModel CreateUnsupportedModel(string requestId, string requestedModelId)
    {
        if (string.IsNullOrWhiteSpace(requestedModelId))
        {
            return new UnsupportedModel
            {
                RequestId = requestId,
                ModelId = string.Empty,
                Message = "The requested model is not available on this host."
            };
        }

        return new UnsupportedModel
        {
            RequestId = requestId,
            ModelId = requestedModelId,
            Message = "The requested model is not available on this host."
        };
    }

    private static ProtocolError CreateProtocolError(
        string? requestId,
        ProtocolErrorCode code,
        string message,
        bool retryable) =>
        new()
        {
            RequestId = requestId ?? string.Empty,
            Code = code,
            Message = message,
            Retryable = retryable
        };

    private static void RequireIdentifier(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty identifier is required.", parameterName);
        }
    }
}
