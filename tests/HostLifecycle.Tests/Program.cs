using System.Net;
using System.Text;
using System.Text.Json;
using Google.Protobuf;
using WhiteP.Shared.Contracts.V1;
using ProtoHostStatus = WhiteP.Shared.Contracts.V1.HostStatus;
using WhiteP.Host.Application;
using WhiteP.Host.Domain;
using WhiteP.Host.Infrastructure;
using WhiteP.Shared.Contracts;
static void Assert(bool ok,string message){if(!ok)throw new Exception(message);}
static void ExpectInvalid(Action action){try{action();}catch(InvalidOperationException){return;}throw new Exception("Invalid transition accepted.");}
static void AssertProtoRoundTrip<T>(string messageName, T message, Func<byte[], T> parse) where T : IMessage<T>
{
    var encoded = message.ToByteArray();
    var decoded = parse(encoded);
    Assert(message.Equals(decoded), $"{messageName} fields changed after Protobuf round-trip.");
    Assert(encoded.SequenceEqual(decoded.ToByteArray()), $"{messageName} re-serialization changed its wire bytes.");
}

var supportedProtocol = ProtocolVersionPolicy.Evaluate(ProtocolVersionPolicy.CurrentVersion);
Assert(supportedProtocol.IsCompatible, "The exact current protocol version must be accepted.");
Assert(supportedProtocol.Status == ProtocolVersionStatus.Compatible, "Current version must return Compatible.");
Assert(supportedProtocol.ErrorCodeName is null, "Accepted versions must not return a rejection code.");

foreach (var malformedVersion in new string?[] { null, "", " ", "1.0", "v1.0.0", "01.0.0", "1.0.0-beta", new string('1', 33) })
{
    var result = ProtocolVersionPolicy.Evaluate(malformedVersion);
    var expectedStatus = string.IsNullOrWhiteSpace(malformedVersion)
        ? ProtocolVersionStatus.Missing
        : ProtocolVersionStatus.Malformed;
    Assert(result.Status == expectedStatus, $"Malformed/missing version classification failed for '{malformedVersion ?? "<null>"}'.");
    Assert(!result.IsCompatible, "Missing or malformed versions must never be accepted.");
    Assert(result.ErrorCodeName == ProtocolVersionPolicy.InvalidMessageErrorCode, "Missing/malformed versions must map to InvalidMessage.");
}

foreach (var unsupportedVersion in new[] { "1.0.1", "1.1.0", "2.0.0" })
{
    var result = ProtocolVersionPolicy.Evaluate(unsupportedVersion);
    Assert(result.Status == ProtocolVersionStatus.Unsupported, $"Incompatible version '{unsupportedVersion}' must be rejected.");
    Assert(!result.IsCompatible, "Incompatible versions must never be accepted.");
    Assert(result.ErrorCodeName == ProtocolVersionPolicy.UnsupportedVersionErrorCode, "Incompatible versions must map to UnsupportedVersion.");
}

var validParserRequest = new InferenceRequest
{
    RequestId = "parser-request",
    AttemptId = "parser-attempt",
    ModelId = "fixture-model",
    Prompt = "valid prompt",
    DeadlineUnixMs = 1770000000999,
    MaxOutputTokens = 64
};
var validParsedRequest = ProtocolMessageParser.ParseInferenceRequest(validParserRequest.ToByteArray());
Assert(validParsedRequest.IsValid, "A complete in-limit inference request must parse and validate.");
Assert(validParsedRequest.Message!.RequestId == "parser-request", "The parser must return the validated request.");

var missingFieldRequest = new InferenceRequest
{
    RequestId = "incomplete-request",
    ModelId = "fixture-model",
    Prompt = "valid prompt",
    DeadlineUnixMs = 1770000000999,
    MaxOutputTokens = 64
};
var missingFieldResult = ProtocolMessageParser.ParseInferenceRequest(missingFieldRequest.ToByteArray());
Assert(!missingFieldResult.IsValid, "A request missing attempt_id must be rejected.");
Assert(missingFieldResult.Validation.ErrorCodeName == ProtocolVersionPolicy.InvalidMessageErrorCode, "Incomplete request must map to InvalidMessage.");

var zeroTokenRequest = validParserRequest.Clone();
zeroTokenRequest.MaxOutputTokens = 0;
var zeroTokenResult = ProtocolMessageParser.ParseInferenceRequest(zeroTokenRequest.ToByteArray());
Assert(!zeroTokenResult.IsValid, "A request with max_output_tokens zero must be rejected.");
Assert(zeroTokenResult.Validation.ErrorCodeName == ProtocolVersionPolicy.InvalidMessageErrorCode, "Zero output tokens must map to InvalidMessage.");

var missingDeadlineRequest = validParserRequest.Clone();
missingDeadlineRequest.DeadlineUnixMs = 0;
var missingDeadlineResult = ProtocolMessageParser.ParseInferenceRequest(missingDeadlineRequest.ToByteArray());
Assert(!missingDeadlineResult.IsValid, "A request with no deadline must be rejected.");

var oversizedWireRequest = new byte[ProtocolMessageLimits.MaximumRequestMessageBytes + 1];
var oversizedWireResult = ProtocolMessageParser.ParseInferenceRequest(oversizedWireRequest);
Assert(!oversizedWireResult.IsValid, "An oversized raw request must be rejected before Protobuf parsing.");
Assert(oversizedWireResult.Validation.Kind == ProtocolMessageValidationKind.ResourceLimit, "An oversized request must return ResourceLimit.");
Assert(oversizedWireResult.Validation.ErrorCodeName == ProtocolMessageLimits.ResourceLimitErrorCode, "An oversized request must map to the resource-limit error code.");

using (var malformedFixtures = JsonDocument.Parse(File.ReadAllText(FindRepositoryFile("tests/contracts/invalid-message-cases.json"))))
{
    var malformedCases = malformedFixtures.RootElement.GetProperty("cases");
    Assert(malformedCases.GetArrayLength() == 3, "The malformed wire corpus must contain all documented fixtures.");
    foreach (var fixture in malformedCases.EnumerateArray())
    {
        var name = fixture.GetProperty("id").GetString()!;
        var wire = Convert.FromBase64String(fixture.GetProperty("wireBase64").GetString()!);
        var expectedCode = fixture.GetProperty("expectedErrorCode").GetString();
        var result = ProtocolMessageParser.ParseInferenceRequest(wire);
        Assert(!result.IsValid, $"Malformed wire fixture '{name}' must be rejected.");
        Assert(result.Validation.ErrorCodeName == expectedCode, $"Malformed wire fixture '{name}' returned the wrong error code.");
    }
}

var validAnnouncement = new HostAnnouncement
{
    EphemeralHostId = "host-parser",
    ProtocolVersion = ProtocolVersionPolicy.CurrentVersion,
    AnnouncedAtUnixMs = 1770000000123,
    Capabilities = new HostCapabilities
    {
        EphemeralHostId = "host-parser",
        ProtocolVersion = ProtocolVersionPolicy.CurrentVersion,
        MaxConcurrentRequests = 1,
        MaxContextCapsuleBytes = ProtocolMessageLimits.MaximumContextCapsuleBytes,
        MaxPromptBytes = ProtocolMessageLimits.MaximumPromptUtf8Bytes
    }
};
validAnnouncement.Capabilities.Models.Add(new ModelCapability
{
    ModelId = "fixture-model",
    DisplayName = "Fixture model",
    MaxContextTokens = 8192,
    MaxOutputTokens = 1024
});
var parsedAnnouncement = ProtocolMessageParser.ParseHostAnnouncement(validAnnouncement.ToByteArray());
Assert(parsedAnnouncement.IsValid, "A complete supported HostAnnouncement must be accepted.");

var unsupportedAnnouncement = validAnnouncement.Clone();
unsupportedAnnouncement.ProtocolVersion = "2.0.0";
unsupportedAnnouncement.Capabilities.ProtocolVersion = "2.0.0";
var unsupportedAnnouncementResult = ProtocolMessageParser.ParseHostAnnouncement(unsupportedAnnouncement.ToByteArray());
Assert(!unsupportedAnnouncementResult.IsValid, "An announcement with an incompatible protocol version must be rejected.");
Assert(unsupportedAnnouncementResult.Validation.Kind == ProtocolMessageValidationKind.UnsupportedVersion, "Incompatible announcement must preserve UnsupportedVersion classification.");
Assert(unsupportedAnnouncementResult.Validation.ErrorCodeName == ProtocolVersionPolicy.UnsupportedVersionErrorCode, "Incompatible announcement must map to UnsupportedVersion.");

var malformedVersionAnnouncement = validAnnouncement.Clone();
malformedVersionAnnouncement.ProtocolVersion = "v1.0.0";
malformedVersionAnnouncement.Capabilities.ProtocolVersion = "v1.0.0";
var malformedVersionResult = ProtocolMessageParser.ParseHostAnnouncement(malformedVersionAnnouncement.ToByteArray());
Assert(!malformedVersionResult.IsValid, "A malformed version string must be rejected.");
Assert(malformedVersionResult.Validation.ErrorCodeName == ProtocolVersionPolicy.InvalidMessageErrorCode, "Malformed versions must map to InvalidMessage.");

var incompleteAnnouncement = new HostAnnouncement
{
    ProtocolVersion = ProtocolVersionPolicy.CurrentVersion,
    AnnouncedAtUnixMs = 1770000000123
};
var incompleteAnnouncementResult = ProtocolMessageParser.ParseHostAnnouncement(incompleteAnnouncement.ToByteArray());
Assert(!incompleteAnnouncementResult.IsValid, "An announcement missing host identity/capabilities must be rejected.");

var oversizedAnnouncement = new byte[ProtocolMessageLimits.MaximumControlMessageBytes + 1];
var oversizedAnnouncementResult = ProtocolMessageParser.ParseHostAnnouncement(oversizedAnnouncement);
Assert(!oversizedAnnouncementResult.IsValid, "An oversized raw control message must be rejected before parsing.");
Assert(oversizedAnnouncementResult.Validation.Kind == ProtocolMessageValidationKind.ResourceLimit, "Oversized control message must map to ResourceLimit.");

var invalidConcurrencyAnnouncement = validAnnouncement.Clone();
invalidConcurrencyAnnouncement.Capabilities.MaxConcurrentRequests = 2;
var invalidConcurrencyResult = ProtocolMessageParser.ParseHostAnnouncement(invalidConcurrencyAnnouncement.ToByteArray());
Assert(!invalidConcurrencyResult.IsValid, "Host capabilities that violate the single-request host constraint must be rejected.");

var invalidModelAnnouncement = validAnnouncement.Clone();
invalidModelAnnouncement.Capabilities.Models[0].MaxOutputTokens = 0;
var invalidModelResult = ProtocolMessageParser.ParseHostAnnouncement(invalidModelAnnouncement.ToByteArray());
Assert(!invalidModelResult.IsValid, "A model capability with an invalid output-token ceiling must be rejected.");

var invalidResponseOutcome = ProtocolMessageValidator.ValidateInferenceResponse(
    "request-id",
    "attempt-id",
    "host-id",
    0,
    "generated text",
    1770000000999,
    "fixture-model",
    string.Empty,
    64);
Assert(!invalidResponseOutcome.IsValid, "An unspecified InferenceResponse outcome must be rejected.");
Assert(invalidResponseOutcome.ErrorCodeName == ProtocolVersionPolicy.InvalidMessageErrorCode, "Invalid response enum values must map to InvalidMessage.");

var oversizedResponseMessage = ProtocolMessageValidator.ValidateInferenceResponse(
    "request-id",
    "attempt-id",
    "host-id",
    1,
    "generated text",
    1770000000999,
    "fixture-model",
    string.Empty,
    ProtocolMessageLimits.MaximumResponseMessageBytes + 1L);
Assert(!oversizedResponseMessage.IsValid, "An oversized serialized InferenceResponse must be rejected.");
Assert(oversizedResponseMessage.ErrorCodeName == ProtocolMessageLimits.ResourceLimitErrorCode, "An oversized response must map to ResourceLimit.");

Console.WriteLine("PASS: malformed, incomplete, oversized, incompatible-version and invalid-value messages are rejected.");


var atLimitRequest = ProtocolMessageLimits.ValidateInferenceRequest(
    new string('p', ProtocolMessageLimits.MaximumPromptUtf8Bytes),
    ProtocolMessageLimits.MaximumContextCapsuleBytes,
    ProtocolMessageLimits.MaximumRequestMessageBytes,
    ProtocolMessageLimits.MaximumOutputTokens);
Assert(atLimitRequest.IsValid, "A request at the documented size ceilings must be accepted.");

var tooLargePromptValidation = ProtocolMessageLimits.ValidatePromptText(
    new string('p', ProtocolMessageLimits.MaximumPromptUtf8Bytes + 1));
Assert(tooLargePromptValidation.Violation == ProtocolLimitViolation.PromptTooLarge, "An over-limit UTF-8 prompt must be rejected.");
Assert(tooLargePromptValidation.ErrorCodeName == ProtocolMessageLimits.ResourceLimitErrorCode, "Prompt size overflow must map to ResourceLimit.");

var tooLargeCapsuleValidation = ProtocolMessageLimits.ValidateContextCapsuleBytes(
    ProtocolMessageLimits.MaximumContextCapsuleBytes + 1);
Assert(tooLargeCapsuleValidation.Violation == ProtocolLimitViolation.ContextCapsuleTooLarge, "An over-limit context capsule must be rejected.");

var tooLargeRequestValidation = ProtocolMessageLimits.ValidateRequestMessageBytes(
    ProtocolMessageLimits.MaximumRequestMessageBytes + 1L);
Assert(tooLargeRequestValidation.Violation == ProtocolLimitViolation.RequestMessageTooLarge, "An over-limit serialized request must be rejected.");

var tooManyOutputTokensValidation = ProtocolMessageLimits.ValidateInferenceRequest(
    "valid prompt",
    0,
    64,
    ProtocolMessageLimits.MaximumOutputTokens + 1);
Assert(tooManyOutputTokensValidation.Violation == ProtocolLimitViolation.OutputTokenLimitExceeded, "An output-token request above the existing backend ceiling must be rejected.");

var maximumResponseText = new string('r', ProtocolMessageLimits.MaximumResponseTextUtf8Bytes);
var maximumResponseValidation = ProtocolMessageLimits.ValidateInferenceResponse(
    maximumResponseText,
    ProtocolMessageLimits.MaximumResponseMessageBytes);
Assert(maximumResponseValidation.IsValid, "A response at the documented size ceilings must be accepted.");

var tooLargeResponseValidation = ProtocolMessageLimits.ValidateResponseText(
    new string('r', ProtocolMessageLimits.MaximumResponseTextUtf8Bytes + 1));
Assert(tooLargeResponseValidation.Violation == ProtocolLimitViolation.ResponseTextTooLarge, "An over-limit UTF-8 response must be rejected.");

Assert(ProtocolMessageLimits.ValidateLocalRequestBodyBytes(ProtocolMessageLimits.MaximumLocalRequestBodyBytes).IsValid, "A local inference request body at the ceiling must be accepted.");
Assert(!ProtocolMessageLimits.ValidateLocalRequestBodyBytes(ProtocolMessageLimits.MaximumLocalRequestBodyBytes + 1L).IsValid, "An oversized local inference request body must be rejected.");
Assert(ProtocolMessageLimits.ValidateLocalResponseBodyBytes(ProtocolMessageLimits.MaximumLocalResponseBodyBytes).IsValid, "A local inference response body at the ceiling must be accepted.");
Assert(!ProtocolMessageLimits.ValidateLocalResponseBodyBytes(ProtocolMessageLimits.MaximumLocalResponseBodyBytes + 1L).IsValid, "An oversized local inference response body must be rejected.");

var protoModelCapability = new ModelCapability
{
    ModelId = "local/model-1",
    DisplayName = "Fixture model",
    MaxContextTokens = 8192,
    MaxOutputTokens = 1024,
    SupportsStreaming = true
};
AssertProtoRoundTrip("ModelCapability", protoModelCapability, bytes => ModelCapability.Parser.ParseFrom(bytes));

var protoHostCapabilities = new HostCapabilities
{
    EphemeralHostId = "host-ephemeral-01",
    ProtocolVersion = "1.0.0",
    MaxConcurrentRequests = 1,
    MaxContextCapsuleBytes = 4096,
    MaxPromptBytes = 8192
};
protoHostCapabilities.Models.Add(protoModelCapability);
AssertProtoRoundTrip("HostCapabilities", protoHostCapabilities, bytes => HostCapabilities.Parser.ParseFrom(bytes));

var protoHostAnnouncement = new HostAnnouncement
{
    EphemeralHostId = "host-ephemeral-01",
    ProtocolVersion = "1.0.0",
    Capabilities = protoHostCapabilities,
    AnnouncedAtUnixMs = 1770000000123
};
AssertProtoRoundTrip("HostAnnouncement", protoHostAnnouncement, bytes => HostAnnouncement.Parser.ParseFrom(bytes));

var protoInferenceRequest = new InferenceRequest
{
    RequestId = "request-001",
    AttemptId = "attempt-001",
    ModelId = "local/model-1",
    Prompt = "Hello, 世界",
    DeadlineUnixMs = 1770000000999,
    MaxOutputTokens = 128,
    ContextCapsule = ByteString.CopyFrom(new byte[] { 0, 1, 2, 255 })
};
AssertProtoRoundTrip("InferenceRequest", protoInferenceRequest, bytes => InferenceRequest.Parser.ParseFrom(bytes));

var protoInferenceResponse = new InferenceResponse
{
    RequestId = "request-001",
    AttemptId = "attempt-001",
    EphemeralHostId = "host-ephemeral-01",
    Outcome = (InferenceOutcome)1,
    OutputText = "Generated output",
    CompletedAtUnixMs = 1770000000888,
    ModelId = "local/model-1",
    ErrorMessage = string.Empty
};
AssertProtoRoundTrip("InferenceResponse", protoInferenceResponse, bytes => InferenceResponse.Parser.ParseFrom(bytes));

var protoCancelRequest = new CancelRequest
{
    RequestId = "request-001",
    AttemptId = "attempt-001",
    Reason = "caller cancelled"
};
AssertProtoRoundTrip("CancelRequest", protoCancelRequest, bytes => CancelRequest.Parser.ParseFrom(bytes));

var protoHostBusy = new HostBusy
{
    RequestId = "request-001",
    EphemeralHostId = "host-ephemeral-01",
    RetryAfterUnixMs = 1770000001500,
    Message = "Host is busy"
};
AssertProtoRoundTrip("HostBusy", protoHostBusy, bytes => HostBusy.Parser.ParseFrom(bytes));

var protoUnsupportedModel = new UnsupportedModel
{
    RequestId = "request-001",
    ModelId = "missing/model",
    Message = "Model is not installed"
};
AssertProtoRoundTrip("UnsupportedModel", protoUnsupportedModel, bytes => UnsupportedModel.Parser.ParseFrom(bytes));

var protoHostStatusRequest = new HostStatusRequest { EphemeralHostId = "host-ephemeral-01" };
AssertProtoRoundTrip("HostStatusRequest", protoHostStatusRequest, bytes => HostStatusRequest.Parser.ParseFrom(bytes));

var protoHostStatus = new ProtoHostStatus
{
    EphemeralHostId = "host-ephemeral-01",
    State = (HostRuntimeState)2,
    ActiveRequests = 1,
    ObservedAtUnixMs = 1770000000222
};
protoHostStatus.Models.Add(protoModelCapability);
AssertProtoRoundTrip("HostStatus", protoHostStatus, bytes => ProtoHostStatus.Parser.ParseFrom(bytes));

var protoProtocolError = new ProtocolError
{
    Code = (ProtocolErrorCode)2,
    Message = "Unsupported protocol version",
    RequestId = "request-001",
    Retryable = false
};
AssertProtoRoundTrip("ProtocolError", protoProtocolError, bytes => ProtocolError.Parser.ParseFrom(bytes));
Console.WriteLine("PASS: C# Protobuf serialize/deserialize round-trip for all 11 primary messages.");

var sharedFixturePath = FindRepositoryFile("tests/contracts/protobuf-wire-fixtures.txt");
var sharedFixtureCount = 0;
foreach (var fixtureLine in File.ReadAllLines(sharedFixturePath))
{
    if (string.IsNullOrWhiteSpace(fixtureLine) || fixtureLine.StartsWith("#", StringComparison.Ordinal)) continue;
    var columns = fixtureLine.Split('\t');
    Assert(columns.Length == 2, "Shared Protobuf fixture must have two tab-separated columns.");
    var fixtureBytes = Convert.FromBase64String(columns[1]);
    IMessage parsed = columns[0] switch
    {
        "ModelCapability" => ModelCapability.Parser.ParseFrom(fixtureBytes),
        "HostCapabilities" => HostCapabilities.Parser.ParseFrom(fixtureBytes),
        "HostAnnouncement" => HostAnnouncement.Parser.ParseFrom(fixtureBytes),
        "InferenceRequest" => InferenceRequest.Parser.ParseFrom(fixtureBytes),
        "InferenceResponse" => InferenceResponse.Parser.ParseFrom(fixtureBytes),
        "CancelRequest" => CancelRequest.Parser.ParseFrom(fixtureBytes),
        "HostBusy" => HostBusy.Parser.ParseFrom(fixtureBytes),
        "UnsupportedModel" => UnsupportedModel.Parser.ParseFrom(fixtureBytes),
        "HostStatusRequest" => HostStatusRequest.Parser.ParseFrom(fixtureBytes),
        "HostStatus" => ProtoHostStatus.Parser.ParseFrom(fixtureBytes),
        "ProtocolError" => ProtocolError.Parser.ParseFrom(fixtureBytes),
        _ => throw new Exception($"Unknown shared fixture '{columns[0]}'.")
    };
    Assert(fixtureBytes.SequenceEqual(parsed.ToByteArray()), $"{columns[0]} changed shared wire bytes in C#.");
    sharedFixtureCount++;
}
Assert(sharedFixtureCount == 11, $"Expected 11 shared fixtures, found {sharedFixtureCount}.");
Console.WriteLine("PASS: C# parsed all 11 shared cross-language wire fixtures.");

static string FindRepositoryFile(string relativePath)
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory is not null)
    {
        var candidate = Path.Combine(directory.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(candidate)) return candidate;
        directory = directory.Parent;
    }
    throw new FileNotFoundException($"Could not find repository file '{relativePath}'.");
}

var m=new HostLifecycle(); Assert(m.Status.State==HostState.Stopped,"Initial state");
m.Transition(HostState.Starting,"Starting");m.Transition(HostState.Ready,"Ready");m.Transition(HostState.Busy,"Busy");m.Transition(HostState.Ready,"Complete");m.Transition(HostState.Stopped,"Stop");
ExpectInvalid(()=>m.Transition(HostState.Busy,"Invalid"));
var status=new HostStartup(new HostLifecycle(),new UnconfiguredInferenceBackend()).Start();
Assert(status.State==HostState.Error,"Unavailable backend must report Error");
Assert(status.Message.Contains("not configured",StringComparison.OrdinalIgnoreCase),"Reason must be visible");
Assert(status.State!=HostState.Ready,"Unavailable backend must not report Ready");
var availableStatus=new HostStartup(new HostLifecycle(),new AvailableTestBackend()).Start();
Assert(availableStatus.State==HostState.Ready,"Available backend must permit Ready state");
Assert(availableStatus.Message.Contains("ready",StringComparison.OrdinalIgnoreCase),"Ready state must explain backend readiness");
var throwingStatus=new HostStartup(new HostLifecycle(),new ThrowingBackend()).Start();
Assert(throwingStatus.State==HostState.Error,"Backend exception must report Error");
Assert(throwingStatus.Message.Contains("check failed",StringComparison.OrdinalIgnoreCase),"Backend exception must produce a stable user-facing reason");
Assert(!throwingStatus.Message.Contains("secret",StringComparison.OrdinalIgnoreCase),"Backend exception details must not leak into status");
var throwingReasonStatus=new HostStartup(new HostLifecycle(),new ThrowingUnavailableReasonBackend()).Start();
Assert(throwingReasonStatus.State==HostState.Error,"UnavailableReason exception must report Error");
Assert(throwingReasonStatus.Message.Contains("check failed",StringComparison.OrdinalIgnoreCase),"UnavailableReason exception must produce a stable user-facing reason");
Assert(!throwingReasonStatus.Message.Contains("private-reason",StringComparison.OrdinalIgnoreCase),"UnavailableReason exception details must not leak into status");
var emptyReasonStatus=new HostStartup(new HostLifecycle(),new EmptyUnavailableReasonBackend()).Start();
Assert(emptyReasonStatus.State==HostState.Error,"Unavailable backend with an empty reason must report Error");
Assert(!string.IsNullOrWhiteSpace(emptyReasonStatus.Message),"Empty backend reason must be replaced with a stable message");
Assert(emptyReasonStatus.Message=="Local inference backend is unavailable. Host is not ready.","Empty backend reason must use the explicit unavailable fallback");

var handler = new RecordingHandler();
using var client = new HttpClient(handler);
using var backend = new LlamaCppServerBackend(new Uri("http://127.0.0.1:8080/"), "fixture-model", client, TimeSpan.FromSeconds(2));
Assert(await backend.CheckHealthAsync(),"Successful /health must mark backend available");
Assert(handler.LastPath=="health","Health check must call the local backend /health endpoint");
var output=await backend.GenerateAsync("test prompt",64);
Assert(output=="generated fixture output","Generated text must be read from the backend response");
Assert(handler.LastPath=="v1/chat/completions","Inference must call the OpenAI-compatible local chat-completions endpoint");
Assert(handler.LastBody!.Contains("test prompt",StringComparison.Ordinal),"Prompt must be sent to local backend");
Assert(handler.LastBody.Contains("fixture-model",StringComparison.Ordinal),"Configured model must be selected");

var requestsBeforeLongPrompt = handler.InferenceRequestCount;
var longPromptRejected = false;
try
{
    await backend.GenerateAsync(new string('p', ProtocolMessageLimits.MaximumPromptUtf8Bytes + 1), 64);
}
catch (ArgumentOutOfRangeException)
{
    longPromptRejected = true;
}
Assert(longPromptRejected, "The live local backend adapter must reject an oversized prompt.");
Assert(handler.InferenceRequestCount == requestsBeforeLongPrompt, "Oversized prompt must be rejected before an HTTP request is sent.");

handler.GeneratedText = new string('r', ProtocolMessageLimits.MaximumResponseTextUtf8Bytes + 1);
var longResponseRejected = false;
try
{
    await backend.GenerateAsync("test prompt", 64);
}
catch (InvalidDataException)
{
    longResponseRejected = true;
}
Assert(longResponseRejected, "The local backend adapter must reject generated text beyond the UTF-8 response limit.");

handler.GeneratedText = new string('r', ProtocolMessageLimits.MaximumLocalResponseBodyBytes + 128);
var hugeResponseBodyRejected = false;
try
{
    await backend.GenerateAsync("test prompt", 64);
}
catch (InvalidDataException)
{
    hugeResponseBodyRejected = true;
}
Assert(hugeResponseBodyRejected, "The local backend adapter must reject an oversized HTTP response body before JSON deserialization.");
handler.GeneratedText = "generated fixture output";

handler.HealthStatus=HttpStatusCode.ServiceUnavailable;
Assert(!await backend.CheckHealthAsync(),"Unhealthy backend must not be marked available");
Assert(!backend.IsAvailable,"Failed health check must clear readiness");
var badInput=false; try { await backend.GenerateAsync(" ",32); } catch(ArgumentException) { badInput=true; }
Assert(badInput,"Empty prompts must be rejected before network access");
using (var session = new HostInferenceSession(backend: new SessionTestBackend()))
{
    var connected = await session.CheckBackendAsync();
    Assert(connected.State == HostState.Ready, "Session should become Ready after successful backend health check.");
    var generated = await session.GenerateAsync("session prompt", 32);
    Assert(generated == "session response", "Session should return generated text.");
    Assert(session.Status.State == HostState.Ready, "Session should return to Ready after generation.");
}
var limitedSessionBackend = new SessionTestBackend();
using (var limitedSession = new HostInferenceSession(backend: limitedSessionBackend))
{
    var ready = await limitedSession.CheckBackendAsync();
    Assert(ready.State == HostState.Ready, "The bounded session should be ready before validation tests.");

    var oversizedCapsuleRequest = new InferenceRequest
    {
        RequestId = "bounded-request",
        AttemptId = "bounded-attempt",
        ModelId = "fixture-model",
        Prompt = "short prompt",
        DeadlineUnixMs = 1770000000999,
        MaxOutputTokens = 32,
        ContextCapsule = ByteString.CopyFrom(new byte[ProtocolMessageLimits.MaximumContextCapsuleBytes + 1])
    };
    var capsuleRejected = false;
    try
    {
        await limitedSession.GenerateAsync(oversizedCapsuleRequest);
    }
    catch (InvalidDataException)
    {
        capsuleRejected = true;
    }
    Assert(capsuleRejected, "An oversized capsule must be rejected before model execution.");

    var oversizedMessageRequest = new InferenceRequest
    {
        RequestId = new string('i', ProtocolMessageLimits.MaximumRequestMessageBytes + 1),
        AttemptId = "bounded-attempt",
        ModelId = "fixture-model",
        Prompt = "short prompt",
        DeadlineUnixMs = 1770000000999,
        MaxOutputTokens = 32
    };
    var messageRejected = false;
    try
    {
        await limitedSession.GenerateAsync(oversizedMessageRequest);
    }
    catch (InvalidDataException)
    {
        messageRejected = true;
    }
    Assert(messageRejected, "An oversized serialized request must be rejected before model execution.");
    Assert(limitedSessionBackend.GenerationCalls == 0, "Invalid request sizes must not reach the inference backend.");
    Assert(limitedSession.Status.State == HostState.Ready, "Size-invalid requests must be rejected before marking the host busy.");

    var validRequest = new InferenceRequest
    {
        RequestId = "valid-request",
        AttemptId = "valid-attempt",
        ModelId = "fixture-model",
        Prompt = "short prompt",
        DeadlineUnixMs = 1770000000999,
        MaxOutputTokens = 32,
        ContextCapsule = ByteString.CopyFrom(new byte[] { 1, 2, 3 })
    };
    var validOutput = await limitedSession.GenerateAsync(validRequest);
    Assert(validOutput == "session response", "A valid bounded protobuf request must reach inference and return output.");
    Assert(limitedSessionBackend.GenerationCalls == 1, "Exactly the valid request should reach inference.");

    limitedSessionBackend.GeneratedText = maximumResponseText + "r";
    var oversizedSessionResponseRejected = false;
    try
    {
        await limitedSession.GenerateAsync(validRequest);
    }
    catch (InvalidDataException)
    {
        oversizedSessionResponseRejected = true;
    }
    Assert(oversizedSessionResponseRejected, "The host session must reject oversized output from any inference backend.");
    Assert(limitedSession.Status.State == HostState.Error, "An oversized response must not be reported as a successful inference.");
}
Console.WriteLine("PASS: protocol request, capsule, prompt, response, HTTP-body ceilings and pre-inference rejection.");

Console.WriteLine("PASS: lifecycle validation, backend endpoint contracts, health readiness, generated response parsing, input bounds, and host-session lifecycle.");

sealed class RecordingHandler : HttpMessageHandler
{
 public string? LastPath {get;private set;}
 public string? LastBody {get;private set;}
 public HttpStatusCode HealthStatus {get;set;}=HttpStatusCode.OK;
 public string GeneratedText {get;set;}="generated fixture output";
 public int InferenceRequestCount {get;private set;}

 protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
 {
  LastPath=request.RequestUri!.AbsolutePath.TrimStart('/');
  if(request.Method==HttpMethod.Get && LastPath=="health") return new HttpResponseMessage(HealthStatus);
  InferenceRequestCount++;
  LastBody=await request.Content!.ReadAsStringAsync(cancellationToken);
  var payload = JsonSerializer.Serialize(new
  {
   choices = new[] { new { message = new { role = "assistant", content = GeneratedText } } }
  });
  return new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(payload,Encoding.UTF8,"application/json")};
 }
}
sealed class AvailableTestBackend : IInferenceBackend { public bool IsAvailable=>true; public string UnavailableReason=>"Test backend is available."; }
sealed class ThrowingBackend : IInferenceBackend { public bool IsAvailable=>throw new InvalidOperationException("secret backend details"); public string UnavailableReason=>throw new InvalidOperationException("secret backend details"); }
sealed class ThrowingUnavailableReasonBackend : IInferenceBackend { public bool IsAvailable=>false; public string UnavailableReason=>throw new InvalidOperationException("private-reason details"); }
sealed class EmptyUnavailableReasonBackend : IInferenceBackend { public bool IsAvailable=>false; public string UnavailableReason=>"   "; }

sealed class SessionTestBackend : IInferenceBackend
{
 public bool IsAvailable=>true;
 public string UnavailableReason=>string.Empty;
 public string GeneratedText {get;set;}="session response";
 public int GenerationCalls {get;private set;}
 public Task<bool> CheckHealthAsync(CancellationToken cancellationToken=default)=>Task.FromResult(true);
 public Task<string> GenerateAsync(string prompt,uint maxOutputTokens,CancellationToken cancellationToken=default)
 {
  GenerationCalls++;
  return Task.FromResult(GeneratedText);
 }
}
