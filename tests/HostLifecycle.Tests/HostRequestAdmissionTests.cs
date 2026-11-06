using WhiteP.Host.Domain;
using WhiteP.Host.Windows.Requests;
internal static class HostRequestAdmissionTests
{
 public static async Task RunAsync()
 {
  var now=DateTimeOffset.FromUnixTimeMilliseconds(1_800_000_000_000);
  var valid=new HostRequestAdmissionContext(true,HostState.Ready,"fixture/model-a","fixture/model-a",HostCapacityState.Ready,(ulong)now.AddMinutes(1).ToUnixTimeMilliseconds());
  Check(valid,now,HostRequestAdmissionFailure.None);
  Check(valid with{HasActiveParticipation=false},now,HostRequestAdmissionFailure.InactiveParticipation);
  Check(valid with{HostState=HostState.Busy},now,HostRequestAdmissionFailure.ModelNotReady);
  Check(valid with{HostState=HostState.Stopped},now,HostRequestAdmissionFailure.ModelNotReady);
  Check(valid with{RequestedModelId="fixture/model-b"},now,HostRequestAdmissionFailure.ModelMismatch);
  Check(valid with{ReadyModelId=null},now,HostRequestAdmissionFailure.ModelMismatch);
  Check(valid with{CapacityState=HostCapacityState.Unknown},now,HostRequestAdmissionFailure.CapacityUnknown);
  Check(valid with{CapacityState=HostCapacityState.Insufficient},now,HostRequestAdmissionFailure.CapacityInsufficient);
  Check(valid with{DeadlineUnixMs=0},now,HostRequestAdmissionFailure.InvalidDeadline);
  Check(valid with{DeadlineUnixMs=(ulong)now.ToUnixTimeMilliseconds()},now,HostRequestAdmissionFailure.DeadlineExpired);
  Check(valid with{DeadlineUnixMs=(ulong)DateTimeOffset.MaxValue.ToUnixTimeMilliseconds()+1UL},now,HostRequestAdmissionFailure.InvalidDeadline);
  var coordinator=new HostRequestCoordinator();var invoked=false;
  var rejected=await coordinator.TryExecuteAsync(valid with{HasActiveParticipation=false},_=>{invoked=true;return Task.FromResult("no");});
  Assert(rejected.Status==HostRequestStatus.Rejected&&rejected.AdmissionFailure==HostRequestAdmissionFailure.InactiveParticipation&&!invoked,"Inactive participation must reject before invocation.");
  var capacityRejected=false;
  try{await coordinator.ExecuteAsync(valid with{CapacityState=HostCapacityState.Unknown},_=>{invoked=true;return Task.FromResult("no");});}
  catch(HostRequestAdmissionException ex){capacityRejected=ex.Failure==HostRequestAdmissionFailure.CapacityUnknown;}
  Assert(capacityRejected&&!invoked,"Unknown capacity must fail closed.");
  var live=new HostRequestAdmissionContext(true,HostState.Ready,"fixture/model-a","fixture/model-a",HostCapacityState.Ready,(ulong)DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeMilliseconds());
  Assert(await coordinator.ExecuteAsync(live,_=>Task.FromResult("accepted"))=="accepted","Valid admission should pass.");
  Console.WriteLine("PASS: admission rejects inactive participation, unready/mismatched model, unknown/insufficient capacity and invalid/expired deadline before inference.");
 }
 private static void Check(HostRequestAdmissionContext c,DateTimeOffset n,HostRequestAdmissionFailure expected){var d=HostRequestAdmissionPolicy.Evaluate(c,n);Assert(d.IsAdmitted==(expected==HostRequestAdmissionFailure.None)&&d.Failure==expected,$"Expected {expected}, actual {d.Failure}.");}
 private static void Assert(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
}
