using WhiteP.Host.Domain;
namespace WhiteP.Host.Windows.Requests;
public enum HostRequestStatus { Completed, Busy, Rejected }
public sealed record HostRequestOutcome<T>(HostRequestStatus Status,T? Result,HostRequestAdmissionFailure? AdmissionFailure=null,string? Message=null)
{
    public static HostRequestOutcome<T> Completed(T result)=>new(HostRequestStatus.Completed,result);
    public static HostRequestOutcome<T> Busy()=>new(HostRequestStatus.Busy,default);
    public static HostRequestOutcome<T> Rejected(HostRequestAdmissionFailure failure,string message)=>new(HostRequestStatus.Rejected,default,failure,message);
}
public sealed class HostRequestBusyException:InvalidOperationException
{
    public HostRequestBusyException():base("Host is Busy; another inference request is already active."){}
    public HostRequestStatus Status=>HostRequestStatus.Busy;
}
public sealed class HostRequestAdmissionException:InvalidOperationException
{
    public HostRequestAdmissionException(HostRequestAdmissionFailure failure,string message):base(message)=>Failure=failure;
    public HostRequestAdmissionFailure Failure{get;}
    public HostRequestStatus Status=>HostRequestStatus.Rejected;
}
public sealed class HostRequestCoordinator
{
    private readonly SemaphoreSlim _inferenceSemaphore=new(1,1);
    public bool IsBusy=>_inferenceSemaphore.CurrentCount==0;
    public async Task<T> ExecuteAsync<T>(HostRequestAdmissionContext admission,Func<CancellationToken,Task<T>> operation,CancellationToken cancellationToken=default)
    {
        var outcome=await TryExecuteAsync(admission,operation,cancellationToken).ConfigureAwait(false);
        if(outcome.Status==HostRequestStatus.Busy)throw new HostRequestBusyException();
        if(outcome.Status==HostRequestStatus.Rejected)throw new HostRequestAdmissionException(outcome.AdmissionFailure??HostRequestAdmissionFailure.InvalidDeadline,outcome.Message??"Request rejected by admission policy.");
        return outcome.Result!;
    }
    public async Task<HostRequestOutcome<T>> TryExecuteAsync<T>(HostRequestAdmissionContext admission,Func<CancellationToken,Task<T>> operation,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(admission);ArgumentNullException.ThrowIfNull(operation);cancellationToken.ThrowIfCancellationRequested();
        var decision=HostRequestAdmissionPolicy.Evaluate(admission,DateTimeOffset.UtcNow);
        if(!decision.IsAdmitted)return HostRequestOutcome<T>.Rejected(decision.Failure,decision.Message);
        if(!await _inferenceSemaphore.WaitAsync(0,cancellationToken).ConfigureAwait(false))return HostRequestOutcome<T>.Busy();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var finalDecision=HostRequestAdmissionPolicy.Evaluate(admission,DateTimeOffset.UtcNow);
            if(!finalDecision.IsAdmitted)return HostRequestOutcome<T>.Rejected(finalDecision.Failure,finalDecision.Message);
            return HostRequestOutcome<T>.Completed(await operation(cancellationToken).ConfigureAwait(false));
        }
        finally{_inferenceSemaphore.Release();}
    }
}
