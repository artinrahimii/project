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
Console.WriteLine("PASS: lifecycle transitions, invalid-transition rejection, unavailable-backend startup.");
