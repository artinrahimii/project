namespace WhiteP.Host.Windows;
public enum HostState { Stopped, Starting, Ready, Busy, Error }
public sealed record HostStatus(HostState State, string Message);
public sealed class HostLifecycle
{
 private static readonly IReadOnlyDictionary<HostState,HostState[]> Allowed = new Dictionary<HostState,HostState[]> {
 [HostState.Stopped]=[HostState.Starting],[HostState.Starting]=[HostState.Ready,HostState.Error,HostState.Stopped],
 [HostState.Ready]=[HostState.Busy,HostState.Error,HostState.Stopped],[HostState.Busy]=[HostState.Ready,HostState.Error],
 [HostState.Error]=[HostState.Starting,HostState.Stopped]};
 public HostStatus Status {get;private set;}=new(HostState.Stopped,"Host stopped.");
 public HostStatus Transition(HostState next,string message) {
 ArgumentException.ThrowIfNullOrWhiteSpace(message);
 if(!Allowed[Status.State].Contains(next)) throw new InvalidOperationException($"Invalid host lifecycle transition: {Status.State} -> {next}.");
 return Status=new HostStatus(next,message.Trim()); }
}
public interface IInferenceBackend { bool IsAvailable {get;} string UnavailableReason {get;} }
public sealed class HostStartup(HostLifecycle lifecycle,IInferenceBackend backend) {
 public HostStatus Start() {
 lifecycle.Transition(HostState.Starting,"Checking local inference backend.");
 return backend.IsAvailable ? lifecycle.Transition(HostState.Ready,"Local inference backend is ready.") : lifecycle.Transition(HostState.Error,backend.UnavailableReason);
 }}
public sealed class UnconfiguredInferenceBackend : IInferenceBackend {
 public bool IsAvailable=>false;
 public string UnavailableReason=>"Local inference backend is not configured. Host is not ready.";
}
