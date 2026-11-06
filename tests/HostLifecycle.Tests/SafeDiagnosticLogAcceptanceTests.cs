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

        string observedRecord = string.Join(" ", new[] { record.Message ?? string.Empty }
            .Concat(payload.Select(value => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty)));
        foreach (var marker in new[]
        {
            "T0187_PROMPT_CANARY_7F2A",
            "T0187_CONTEXT_CAPSULE_CANARY_91C4",
            "T0187_SECRET_CANARY_3BD8"
        })
        {
            if (observedRecord.Contains(marker, StringComparison.Ordinal))
                throw new InvalidOperationException("A sensitive canary appeared in the emitted safe diagnostic event.");
        }

        Console.WriteLine("PASS: safe diagnostic EventSource emitted numeric allowlist fields and no prompt/capsule/secret canaries.");
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
