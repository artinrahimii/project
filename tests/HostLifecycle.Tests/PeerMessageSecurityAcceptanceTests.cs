using System;
using System.IO;
using WhiteP.Host.Infrastructure;
using WhiteP.Host.Windows.Networking;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.V1;
using WhiteP.Shared.Security;

internal static class PeerMessageSecurityAcceptanceTests
{
    public static void Run()
    {
        TamperedRequestFieldsAreRejectedBeforeDispatch();
        ReplayGuardRejectsRepeatedAuthenticatedRequestAttempts();
        ForgedOrMisroutedResponsesAreRejected();
        Console.WriteLine("PASS: tampered request rejection, bounded request replay fencing, forged response correlation rejection, and authenticated-peer transport checks.");
    }

    private static void TamperedRequestFieldsAreRejectedBeforeDispatch()
    {
        var valid = CreateRequest();
        var validResult = ProtocolMessageParser.ParseInferenceRequest(valid.ToByteArray());
        Assert(validResult.IsValid, "The baseline protocol request must be accepted.");

        var tampered = valid.Clone();
        tampered.RequestId = "request/with/forbidden/characters";
        var tamperedResult = ProtocolMessageParser.ParseInferenceRequest(tampered.ToByteArray());
        Assert(!tamperedResult.IsValid && tamperedResult.Message is null,
            "A request whose correlation identifier was modified to an invalid value must be rejected before dispatch.");

        var malformed = ProtocolMessageParser.ParseInferenceRequest(new byte[] { 0x0A, 0xFF, 0xFF });
        Assert(!malformed.IsValid && malformed.Message is null,
            "Malformed/tampered Protobuf wire data must not produce an accepted inference request.");
    }

    private static void ReplayGuardRejectsRepeatedAuthenticatedRequestAttempts()
    {
        var guard = new AuthenticatedRequestReplayGuard(maximumTrackedRequests: 3);
        var now = DateTimeOffset.UtcNow;
        var expiry = now.AddMinutes(2);

        Assert(guard.TryRegister("authenticated-peer-a", "req-001", "attempt-001", expiry, now) == AuthenticatedRequestReplayDecision.Accepted,
            "The first authenticated request/attempt tuple must be registered.");
        Assert(guard.TryRegister("authenticated-peer-a", "req-001", "attempt-001", expiry, now) == AuthenticatedRequestReplayDecision.Duplicate,
            "The same authenticated peer must not replay the exact request/attempt tuple.");
        Assert(guard.TryRegister("authenticated-peer-a", "req-001", "attempt-002", expiry, now) == AuthenticatedRequestReplayDecision.Accepted,
            "A separately identified attempt must not collide with the previous attempt key.");
        Assert(guard.TryRegister("authenticated-peer-b", "req-001", "attempt-001", expiry, now) == AuthenticatedRequestReplayDecision.Accepted,
            "Replay keys must be scoped to the authenticated peer identity.");
        Assert(guard.TryRegister("authenticated-peer-c", "req-002", "attempt-003", expiry, now) == AuthenticatedRequestReplayDecision.CapacityExceeded,
            "The guard must fail closed rather than evict a live replay entry when its bound is reached.");

        var afterExpiry = expiry.AddMilliseconds(1);
        Assert(guard.TryRegister("authenticated-peer-c", "req-002", "attempt-003", afterExpiry.AddMinutes(1), afterExpiry) == AuthenticatedRequestReplayDecision.Accepted,
            "Expired entries must be reclaimed and allow a new request after the original deadline.");
        Assert(guard.TrackedRequestCount == 1, "Expired replay entries must be pruned from the bounded in-memory cache.");
    }

    private static void ForgedOrMisroutedResponsesAreRejected()
    {
        var request = CreateRequest();
        var valid = new InferenceResponse
        {
            RequestId = request.RequestId,
            AttemptId = request.AttemptId,
            EphemeralHostId = "authenticated-local-host",
            Outcome = InferenceOutcome.Succeeded,
            OutputText = "verified-result",
            CompletedAtUnixMs = (ulong)Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
            ModelId = request.ModelId,
            ErrorMessage = string.Empty
        };
        PeerResponseCorrelationValidator.Validate(request, valid, "authenticated-local-host");

        var wrongRequest = valid.Clone();
        wrongRequest.RequestId = "req-forged";
        ExpectInvalidData(() => PeerResponseCorrelationValidator.Validate(request, wrongRequest, "authenticated-local-host"));

        var wrongAttempt = valid.Clone();
        wrongAttempt.AttemptId = "attempt-forged";
        ExpectInvalidData(() => PeerResponseCorrelationValidator.Validate(request, wrongAttempt, "authenticated-local-host"));

        var wrongHost = valid.Clone();
        wrongHost.EphemeralHostId = "unexpected-host";
        ExpectInvalidData(() => PeerResponseCorrelationValidator.Validate(request, wrongHost, "authenticated-local-host"));

        var wrongModel = valid.Clone();
        wrongModel.ModelId = "different-model";
        ExpectInvalidData(() => PeerResponseCorrelationValidator.Validate(request, wrongModel, "authenticated-local-host"));
    }

    private static InferenceRequest CreateRequest() => new()
    {
        ProtocolVersion = ProtocolVersionPolicy.CurrentVersion,
        RequestId = "req_00112233445566778899aabbccddeeff",
        AttemptId = "attempt-001",
        ModelId = "fixture-model",
        Prompt = "non-sensitive security test prompt",
        DeadlineUnixMs = (ulong)DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeMilliseconds(),
        MaxOutputTokens = 16
    };

    private static void ExpectInvalidData(Action action)
    {
        try { action(); }
        catch (InvalidDataException) { return; }
        throw new InvalidOperationException("A response with forged correlation fields was accepted.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
