using System.Net;
using System.Text;
using Google.Protobuf;
using WhiteP.Shared.Contracts.V1;
using WhiteP.Host.Windows;
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

var protoHostStatus = new HostStatus
{
    EphemeralHostId = "host-ephemeral-01",
    State = (HostRuntimeState)2,
    ActiveRequests = 1,
    ObservedAtUnixMs = 1770000000222
};
protoHostStatus.Models.Add(protoModelCapability);
AssertProtoRoundTrip("HostStatus", protoHostStatus, bytes => HostStatus.Parser.ParseFrom(bytes));

var protoProtocolError = new ProtocolError
{
    Code = (ProtocolErrorCode)2,
    Message = "Unsupported protocol version",
    RequestId = "request-001",
    Retryable = false
};
AssertProtoRoundTrip("ProtocolError", protoProtocolError, bytes => ProtocolError.Parser.ParseFrom(bytes));
Console.WriteLine("PASS: C# Protobuf serialize/deserialize round-trip for all 11 primary messages.");

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
Console.WriteLine("PASS: lifecycle validation, backend endpoint contracts, health readiness, generated response parsing, input bounds, and host-session lifecycle.");

sealed class RecordingHandler : HttpMessageHandler
{
 public string? LastPath {get;private set;}
 public string? LastBody {get;private set;}
 public HttpStatusCode HealthStatus {get;set;}=HttpStatusCode.OK;
 protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
 {
  LastPath=request.RequestUri!.AbsolutePath.TrimStart('/');
  if(request.Method==HttpMethod.Get && LastPath=="health") return new HttpResponseMessage(HealthStatus);
  LastBody=await request.Content!.ReadAsStringAsync(cancellationToken);
  return new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent("{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"generated fixture output\"}}]}",Encoding.UTF8,"application/json")};
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
 public Task<bool> CheckHealthAsync(CancellationToken cancellationToken=default)=>Task.FromResult(true);
 public Task<string> GenerateAsync(string prompt,uint maxOutputTokens,CancellationToken cancellationToken=default)=>Task.FromResult("session response");
}
