using System;
using System.Threading.Tasks;
using WhiteP.Host.Windows.Networking;
using WhiteP.Host.Windows.Requests;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.V1;

internal static class NetworkInferenceRequestResponseMapperAcceptanceTests
{
    public static Task RunAsync()
    {
        BusyAndNotReadyFailuresMapToHostBusy();
        ModelMismatchMapsToUnsupportedModel();
        InvalidRequestCodesMapToProtocolErrors();
        CapacityFailureMapsToResourceLimit();
        InternalFailureIsSanitized();
        Console.WriteLine("PASS: host admission and validation failures map to stable Protobuf response contract types.");
        return Task.CompletedTask;
    }

    private static void BusyAndNotReadyFailuresMapToHostBusy()
    {
        var now = DateTimeOffset.FromUnixTimeMilliseconds(1_800_000_000_000);
        var busy = NetworkInferenceRequestResponseMapper.MapAdmissionOutcome(
            HostRequestOutcome<string>.Busy(), "request-busy", "model-a", "host-a", now);
        Assert(busy is HostBusy, "A Busy admission outcome must map to the HostBusy contract.");
        var response = (HostBusy)busy;
        Assert(response.RequestId == "request-busy", "HostBusy must preserve the request ID.");
        Assert(response.EphemeralHostId == "host-a", "HostBusy must identify the responding host.");
        Assert(response.RetryAfterUnixMs > (ulong)now.ToUnixTimeMilliseconds(), "HostBusy must suggest a future retry time.");

        var notReady = NetworkInferenceRequestResponseMapper.MapAdmissionOutcome(
            HostRequestOutcome<string>.Rejected(HostRequestAdmissionFailure.ModelNotReady, "private-state"),
            "request-not-ready", "model-a", "host-a", now);
        Assert(notReady is HostBusy, "A not-ready host must map to a retryable busy-state contract rather than a model mismatch.");
        Assert(((HostBusy)notReady).Message == "The host is busy or temporarily unavailable.", "Busy messages must be stable and sanitized.");
    }

    private static void ModelMismatchMapsToUnsupportedModel()
    {
        var outcome = HostRequestOutcome<string>.Rejected(
            HostRequestAdmissionFailure.ModelMismatch, "internal model lookup detail");
        var mapped = NetworkInferenceRequestResponseMapper.MapAdmissionOutcome(
            outcome, "request-model", "missing/model", "host-a",
            DateTimeOffset.FromUnixTimeMilliseconds(1_800_000_000_000));
        Assert(mapped is UnsupportedModel, "A model mismatch must map to the UnsupportedModel contract.");
        var response = (UnsupportedModel)mapped;
        Assert(response.RequestId == "request-model" && response.ModelId == "missing/model", "UnsupportedModel must preserve request/model identifiers.");
        Assert(!response.Message.Contains("internal model lookup detail", StringComparison.Ordinal), "UnsupportedModel must not expose internal admission details.");
    }

    private static void InvalidRequestCodesMapToProtocolErrors()
    {
        var invalid = NetworkInferenceRequestResponseMapper.CreateInvalidRequest(
            "request-invalid",
            new ProtocolMessageValidationResult(
                ProtocolMessageValidationKind.InvalidMessage,
                ProtocolVersionPolicy.InvalidMessageErrorCode,
                "untrusted request text"));
        Assert(invalid.Code == ProtocolErrorCode.InvalidMessage && invalid.RequestId == "request-invalid", "Invalid requests must map to ProtocolError INVALID_MESSAGE.");
        Assert(!invalid.Retryable, "Invalid requests must not be marked retryable.");
        Assert(!invalid.Message.Contains("untrusted request text", StringComparison.Ordinal), "Validation details must not echo untrusted request content.");

        var unsupported = NetworkInferenceRequestResponseMapper.CreateInvalidRequest(
            "request-version",
            new ProtocolMessageValidationResult(
                ProtocolMessageValidationKind.UnsupportedVersion,
                ProtocolVersionPolicy.UnsupportedVersionErrorCode,
                "private-version-detail"));
        Assert(unsupported.Code == ProtocolErrorCode.UnsupportedVersion, "Unsupported request versions must retain the UnsupportedVersion contract code.");

        var expired = NetworkInferenceRequestResponseMapper.CreateInvalidRequest(
            "request-expired",
            new ProtocolMessageValidationResult(
                ProtocolMessageValidationKind.RequestExpired,
                ProtocolMessageValidator.RequestExpiredErrorCode,
                "deadline detail"));
        Assert(expired.Code == ProtocolErrorCode.RequestExpired, "Expired deadlines must map to REQUEST_EXPIRED.");
    }

    private static void CapacityFailureMapsToResourceLimit()
    {
        var mapped = NetworkInferenceRequestResponseMapper.MapAdmissionOutcome(
            HostRequestOutcome<string>.Rejected(HostRequestAdmissionFailure.CapacityInsufficient, "private capacity detail"),
            "request-capacity", "model-a", "host-a",
            DateTimeOffset.FromUnixTimeMilliseconds(1_800_000_000_000));
        Assert(mapped is ProtocolError, "Capacity failure must use the ProtocolError contract.");
        Assert(((ProtocolError)mapped).Code == ProtocolErrorCode.ResourceLimit, "Capacity failure must map to RESOURCE_LIMIT.");
        Assert(!((ProtocolError)mapped).Retryable, "Unknown capacity must fail closed rather than suggest an unsafe automatic retry.");
    }

    private static void InternalFailureIsSanitized()
    {
        var response = NetworkInferenceRequestResponseMapper.CreateInternalError("request-internal");
        Assert(response.Code == ProtocolErrorCode.Internal, "Internal failures must map to PROTOCOL_ERROR_CODE_INTERNAL.");
        Assert(response.RequestId == "request-internal", "Internal error must preserve the request ID.");
        Assert(response.Message == "The host could not process the request.", "Internal errors must use a stable message without exception details.");
        Assert(!response.Retryable, "Internal failures must not be advertised as retryable without evidence that retry is safe.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
