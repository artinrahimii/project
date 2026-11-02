using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using WhiteP.Shared.Security;

internal static class SafeDiagnosticLogAcceptanceTests
{
    private static readonly ConcurrentQueue<EventWrittenEventArgs> Records = new();

    public static void Run()
    {
        while (Records.TryDequeue(out _))
        {
        }

        using var listener = new SafeDiagnosticEventListener();
        SafeDiagnosticLog.Write(
            SafeDiagnosticEventCode.InferenceSubmission,
            SafeDiagnosticOutcomeCode.Succeeded,
            27);

        if (!Records.TryDequeue(out EventWrittenEventArgs? record))
        {
            throw new InvalidOperationException("The safe diagnostic EventSource did not emit its enabled event.");
        }

        if (record.EventId != 1 || record.Payload is not { Count: 3 } payload)
        {
            throw new InvalidOperationException("The safe diagnostic event did not match the three-field schema.");
        }

        if (payload[0] is not int eventCode ||
            payload[1] is not int outcomeCode ||
            payload[2] is not long durationMilliseconds ||
            eventCode != (int)SafeDiagnosticEventCode.InferenceSubmission ||
            outcomeCode != (int)SafeDiagnosticOutcomeCode.Succeeded ||
            durationMilliseconds != 27)
        {
            throw new InvalidOperationException("The safe diagnostic event contained unexpected or non-numeric values.");
        }

        Console.WriteLine("PASS: safe diagnostic EventSource emitted only the three allowlisted numeric fields.");
    }

    private sealed class SafeDiagnosticEventListener : EventListener
    {
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "WhiteP-SafeDiagnostics")
            {
                EnableEvents(eventSource, EventLevel.LogAlways);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            Records.Enqueue(eventData);
        }
    }
}
