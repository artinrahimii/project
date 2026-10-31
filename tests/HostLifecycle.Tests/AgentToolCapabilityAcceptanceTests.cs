using System.Text.Json;
using WhiteP.Host.Application;
using WhiteP.Host.Infrastructure;
using WhiteP.Host.Windows.Chat;
using WhiteP.Host.Windows.Tools;

internal static class AgentToolCapabilityAcceptanceTests
{
    private const string EmptySchema = """{"type":"object","properties":{},"required":[],"additionalProperties":false}""";
    public static async Task RunAsync()
    {
        var textOnly=new TextOnlyCapabilityTestBackend();
        Assert(!(await AgentToolCapabilityPolicy.EvaluateAsync(textOnly)).IsSupported,"Missing provider must fail closed.");
        var unknown=new CapabilityReportTestBackend(Report(AgentToolCapabilitySupport.Supported,AgentToolCapabilitySupport.Unknown,AgentToolCapabilitySupport.Supported,DateTimeOffset.UtcNow,"unknown model"));
        Assert(!(await AgentToolCapabilityPolicy.EvaluateAsync(unknown)).IsSupported,"Unknown model support must fail closed.");
        Assert(!AgentToolCapabilityPolicy.Evaluate(Report(AgentToolCapabilitySupport.Supported,AgentToolCapabilitySupport.Supported,AgentToolCapabilitySupport.Unsupported,DateTimeOffset.UtcNow,"protocol unsupported"),DateTimeOffset.UtcNow).IsSupported,"Unsupported protocol must fail closed.");
        Assert(!AgentToolCapabilityPolicy.Evaluate(Report(AgentToolCapabilitySupport.Supported,AgentToolCapabilitySupport.Supported,AgentToolCapabilitySupport.Supported,DateTimeOffset.UtcNow.AddMinutes(-6),"stale"),DateTimeOffset.UtcNow).IsSupported,"Stale report must fail closed.");
        Assert(!AgentToolCapabilityPolicy.Evaluate(Report(AgentToolCapabilitySupport.Supported,AgentToolCapabilitySupport.Supported,AgentToolCapabilitySupport.Supported,DateTimeOffset.UtcNow," "),DateTimeOffset.UtcNow).IsSupported,"Missing evidence must fail closed.");
        Assert(!(await AgentToolCapabilityPolicy.EvaluateAsync(new CapabilityReportTestBackend(unknown.Report,true))).IsSupported,"Provider exceptions must fail closed.");
        using var llama=new LlamaCppServerBackend(new Uri("http://127.0.0.1:8080/"),"test-model");
        var actual=await llama.CheckAgentToolCallingCapabilitiesAsync();
        Assert(actual.BackendSupport==AgentToolCapabilitySupport.Unknown&&actual.ModelSupport==AgentToolCapabilitySupport.Unknown&&actual.StructuredProtocolSupport==AgentToolCapabilitySupport.Unsupported,"Current text-only llama.cpp adapter must not claim structured tool support.");
        var tool=new CapabilityTestTool();var registry=new AgentToolRegistry(new[]{tool});
        Assert((await registry.GetModelAdvertisableDefinitionsAsync(textOnly)).Count==0,"Unverified backend must not receive tool definitions.");
        var supported=new SupportedAgentToolCallingBackend();
        var definitions=await registry.GetModelAdvertisableDefinitionsAsync(supported);
        Assert(definitions.Count==1&&definitions[0].Name=="get_host_status","A fully supported report plus explicit allowlist may expose definitions.");
        var denyPrompt=new CapabilityTestConfirmationService();var denyDispatch=new CapabilityTestProcessExecutor();
        var disabled=new AgentToolExecutionCoordinator(registry,denyPrompt,denyDispatch,textOnly);
        await ExpectThrowsAsync<AgentToolCapabilityUnavailableException>(()=>disabled.ExecuteAsync(Call("get_host_status","{}")));
        Assert(denyPrompt.RequestCount==0&&denyDispatch.InvocationCount==0,"Capability gate must run before confirmation or process dispatch.");
        var okPrompt=new CapabilityTestConfirmationService();var okDispatch=new CapabilityTestProcessExecutor();
        var enabled=new AgentToolExecutionCoordinator(registry,okPrompt,okDispatch,supported);
        var result=await enabled.ExecuteAsync(Call("get_host_status","{}"));
        Assert(result.Status==AgentToolExecutionStatus.Executed&&result.Result?.Content=="Capability-gated worker result.","Supported capability should permit a valid call.");
        Assert(okPrompt.RequestCount==0&&okDispatch.InvocationCount==1&&tool.ExecutionCount==0,"Dispatch must stay on the worker boundary.");
        Console.WriteLine("PASS: agent tool exposure/execution fail closed without fresh backend, model and structured-protocol support.");
    }
    private static AgentToolCallingCapabilityReport Report(AgentToolCapabilitySupport b,AgentToolCapabilitySupport m,AgentToolCapabilitySupport p,DateTimeOffset t,string e)=>new("test-backend","test-model",b,m,p,t,e);
    private static AgentToolCall Call(string n,string j){using var d=JsonDocument.Parse(j);return new AgentToolCall(n,d.RootElement);}
    private static async Task ExpectThrowsAsync<T>(Func<Task> a) where T:Exception{try{await a();}catch(T){return;}throw new InvalidOperationException("Expected "+typeof(T).Name);}
    private static void Assert(bool c,string m){if(!c)throw new InvalidOperationException(m);}
    private sealed class CapabilityTestTool:IAgentTool{
        public AgentToolDefinition Definition{get;}=new("get_host_status","Test capability-gated tool.",EmptySchema,false);
        public int ExecutionCount{get;private set;}
        public bool ValidateArguments(JsonElement a,out string? error){bool ok=a.ValueKind==JsonValueKind.Object&&!a.EnumerateObject().Any();error=ok?null:"Expected empty object.";return ok;}
        public Task<AgentToolResult> ExecuteAsync(JsonElement a,CancellationToken t){t.ThrowIfCancellationRequested();ExecutionCount++;return Task.FromResult(new AgentToolResult("Should not execute in host."));}
    }
}
internal sealed class TextOnlyCapabilityTestBackend:IInferenceBackend{
 public bool IsAvailable=>true;public string UnavailableReason=>string.Empty;
 public Task<string> GenerateAsync(string p,uint n,CancellationToken t=default){t.ThrowIfCancellationRequested();return Task.FromResult("Text response.");}
}
internal class CapabilityReportTestBackend:IInferenceBackend,IAgentToolCapabilityProvider{
 readonly bool _throw;public CapabilityReportTestBackend(AgentToolCallingCapabilityReport r,bool fail=false){Report=r;_throw=fail;}
 public AgentToolCallingCapabilityReport Report{get;}public bool IsAvailable=>true;public string UnavailableReason=>string.Empty;
 public Task<AgentToolCallingCapabilityReport> CheckAgentToolCallingCapabilitiesAsync(CancellationToken t=default){t.ThrowIfCancellationRequested();if(_throw)throw new InvalidOperationException("Test probe failed.");return Task.FromResult(Report);}
 public Task<string> GenerateAsync(string p,uint n,CancellationToken t=default){t.ThrowIfCancellationRequested();return Task.FromResult("Capability test response.");}
}
internal sealed class SupportedAgentToolCallingBackend:CapabilityReportTestBackend{
 public SupportedAgentToolCallingBackend():base(new AgentToolCallingCapabilityReport("test-backend","test-model",AgentToolCapabilitySupport.Supported,AgentToolCapabilitySupport.Supported,AgentToolCapabilitySupport.Supported,DateTimeOffset.UtcNow,"Test-only fresh supported capability report.")){}
}
internal sealed class CapabilityTestConfirmationService:IAgentToolConfirmationService{
 public int RequestCount{get;private set;}public Task<bool> ConfirmAsync(AgentToolDefinition d,AgentToolCall c,CancellationToken t){t.ThrowIfCancellationRequested();RequestCount++;return Task.FromResult(true);}
}
internal sealed class CapabilityTestProcessExecutor:IAgentToolProcessExecutor{
 public int InvocationCount{get;private set;}public Task<AgentToolResult> ExecuteAsync(AgentToolCall c,CancellationToken t=default){t.ThrowIfCancellationRequested();InvocationCount++;return Task.FromResult(new AgentToolResult("Capability-gated worker result."));}
}
