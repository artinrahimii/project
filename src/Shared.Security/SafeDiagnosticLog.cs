using System;
using System.Diagnostics.Tracing;
using System.Threading;

namespace WhiteP.Shared.Security;

/// <summary>Fixed, non-identifying event categories for diagnostic telemetry.</summary>
public enum SafeDiagnosticEventCode
{
    PeerTransportConnection = 1,
    InferenceSubmission = 2
}

/// <summary>Outcome codes emitted by the safe diagnostic event schema.</summary>
public enum SafeDiagnosticOutcomeCode
{
    Started = 1,
    Succeeded = 2,
    Rejected = 3,
    Failed = 4,
    Cancelled = 5
}

/// <summary>
/// Emits only numeric event and outcome codes plus elapsed milliseconds.
/// No identifiers, addresses, message content, keys, exception values or templates are accepted.
/// </summary>
public static class SafeDiagnosticLog
{
    private const int MaximumRecordsPerProcess = 80;
    private static int _written;

    public static void Write(
        SafeDiagnosticEventCode eventCode,
        SafeDiagnosticOutcomeCode outcomeCode,
        long durationMilliseconds = 0)
    {
        if (!Enum.IsDefined(eventCode))
        {
            throw new ArgumentOutOfRangeException(nameof(eventCode));
        }

        if (!Enum.IsDefined(outcomeCode))
        {
            throw new ArgumentOutOfRangeException(nameof(outcomeCode));
        }

        if (durationMilliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationMilliseconds));
        }

        SafeDiagnosticEventSource source = SafeDiagnosticEventSource.Log;
        if (!source.IsEnabled() || !TryAcquireRecord())
        {
            return;
        }

        source.Record((int)eventCode, (int)outcomeCode, durationMilliseconds);
    }

    private static bool TryAcquireRecord()
    {
        int current = Volatile.Read(ref _written);
        while (current < MaximumRecordsPerProcess)
        {
            int observed = Interlocked.CompareExchange(ref _written, current + 1, current);
            if (observed == current)
            {
                return true;
            }

            current = observed;
        }

        return false;
    }
}

[EventSource(Name = "WhiteP-SafeDiagnostics")]
internal sealed class SafeDiagnosticEventSource : EventSource
{
    internal static readonly SafeDiagnosticEventSource Log = new();

    private SafeDiagnosticEventSource()
    {
    }

    [Event(
        1,
        Level = EventLevel.Informational,
        Message = "schema=1 event_code={0} outcome_code={1} duration_ms={2}")]
    public void Record(int eventCode, int outcomeCode, long durationMilliseconds)
    {
        if (IsEnabled())
        {
            WriteEvent(1, new object?[] { eventCode, outcomeCode, durationMilliseconds });
        }
    }
}
