using System.Net;
using System.Text;
using WhiteP.Host.Windows;
static void Assert(bool ok,string message){if(!ok)throw new Exception(message);}
static void ExpectInvalid(Action action){try{action();}catch(InvalidOperationException){return;}throw new Exception("Invalid transition accepted.");}
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
Console.WriteLine("PASS: lifecycle validation, safe startup errors, real HTTP backend endpoint contracts, health readiness, generated response parsing, and input bounds.");

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
