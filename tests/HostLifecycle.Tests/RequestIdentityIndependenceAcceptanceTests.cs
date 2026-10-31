using System.Text.RegularExpressions;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using WhiteP.Host.Infrastructure;
using WhiteP.Host.Windows.Networking;
using WhiteP.Host.Windows.Requests;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.V1;

internal static class RequestIdentityIndependenceAcceptanceTests
{
    private static readonly Regex RequestIdPattern = new(
        "^req_[0-9a-f]{32}$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static void Run()
    {
        const string userId = "user_ACCOUNT_ID_7B4D";
        const string persistentConversationId = "conversation_PERSISTENT_ID_93AE";
        var identifiers = new HashSet<string>(StringComparer.Ordinal);

        for (var index = 0; index < 512; index++)
        {
            var requestId = SecureRequestIdGenerator.CreateRequestId();
            Assert(RequestIdPattern.IsMatch(requestId),
                "Request IDs must have the opaque req_ + 128-bit lowercase-hex format.");
            Assert(!requestId.Contains(userId, StringComparison.Ordinal) &&
                   !requestId.Contains(persistentConversationId, StringComparison.Ordinal),
                "A request ID must not encode a user ID or a persistent conversation ID.");
            Assert(identifiers.Add(requestId),
                "Independent request generation must not repeat an ID within this regression sample.");
        }

        AssertNoStableIdentityFields(InferenceRequest.Descriptor, "InferenceRequest");
        AssertNoStableIdentityFields(InferenceResponse.Descriptor, "InferenceResponse");
        AssertNoStableIdentityFields(ProtocolError.Descriptor, "ProtocolError");
        AssertNoStableIdentityFields(HostBusy.Descriptor, "HostBusy");
        AssertNoStableIdentityFields(UnsupportedModel.Descriptor, "UnsupportedModel");

        var correlatedRequestId = SecureRequestIdGenerator.CreateRequestId();
        var request = new InferenceRequest
        {
            ProtocolVersion = ProtocolVersionPolicy.CurrentVersion,
            RequestId = correlatedRequestId,
            AttemptId = "attempt_identity_independence",
            ModelId = "fixture-model",
            Prompt = "identity boundary acceptance",
            DeadlineUnixMs = 4_102_444_800_000UL,
            MaxOutputTokens = 16
        };
        var parseResult = ProtocolMessageParser.ParseInferenceRequest(request.ToByteArray());
        Assert(parseResult.IsValid, "A valid opaque request ID must survive normal protocol validation.");
        Assert(parseResult.Message!.RequestId == correlatedRequestId,
            "Protocol parsing must preserve the request correlation ID without converting it to a stable identity.");

        var mappedFailure = NetworkInferenceRequestResponseMapper.CreateInternalError(correlatedRequestId);
        Assert(mappedFailure.RequestId == correlatedRequestId,
            "A typed protocol failure must correlate to the originating request ID.");
        Assert(!mappedFailure.RequestId.Contains(userId, StringComparison.Ordinal) &&
               !mappedFailure.RequestId.Contains(persistentConversationId, StringComparison.Ordinal),
            "Failure correlation must not replace the opaque request ID with account or conversation identity.");

        Console.WriteLine(
            "PASS: request IDs remain opaque and independent of stable user/conversation identifiers; protocol contracts exclude stable IDs and preserve request correlation.");
    }

    private static void AssertNoStableIdentityFields(MessageDescriptor descriptor, string messageType)
    {
        foreach (var fieldName in new[]
                 {
                     "user_id",
                     "account_id",
                     "device_id",
                     "conversation_id",
                     "persistent_conversation_id"
                 })
        {
            Assert(descriptor.FindFieldByName(fieldName) is null,
                $"{messageType} must not expose stable identity field '{fieldName}'.");
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
