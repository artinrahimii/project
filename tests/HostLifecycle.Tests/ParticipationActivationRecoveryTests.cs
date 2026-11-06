using System;
using System.IO;
using System.Threading.Tasks;
using WhiteP.Host.Application;

internal static class ParticipationActivationRecoveryTests
{
    public static async Task RunAsync()
    {
        await SuccessfulRollbackKeepsTheActivationMessageSafeAsync().ConfigureAwait(false);
        await FailedRollbackAddsSafeRecoveryGuidanceAsync().ConfigureAwait(false);
        await DisposalDuringShutdownDoesNotReportASecondFailureAsync().ConfigureAwait(false);

        Console.WriteLine(
            "PASS: participation activation recovery maps errors safely and preserves disposal semantics.");
    }

    private static async Task SuccessfulRollbackKeepsTheActivationMessageSafeAsync()
    {
        var rollbackCalls = 0;
        const string secretActivationDetail = "activation-secret-path-or-token";
        var message = await ParticipationActivationRecovery.RecoverAsync(
            new InvalidOperationException(secretActivationDetail),
            () =>
            {
                rollbackCalls++;
                return Task.CompletedTask;
            }).ConfigureAwait(false);

        Assert(rollbackCalls == 1, "A failed activation must request one rollback.");
        Assert(
            message ==
                "Could not activate network participation: This operation is not available in the current state. Check the host status and try again.",
            "A successful rollback must preserve the safe activation-error message.");
        Assert(!message.Contains(secretActivationDetail, StringComparison.Ordinal),
            "The activation exception detail must not be exposed.");
    }

    private static async Task FailedRollbackAddsSafeRecoveryGuidanceAsync()
    {
        const string secretActivationDetail = "activation-private-detail";
        const string secretRollbackDetail = "rollback-private-file-path";
        var message = await ParticipationActivationRecovery.RecoverAsync(
            new InvalidOperationException(secretActivationDetail),
            () => Task.FromException(new IOException(secretRollbackDetail))).ConfigureAwait(false);

        Assert(message.Contains(
                "Could not activate network participation: This operation is not available in the current state.",
                StringComparison.Ordinal),
            "The original activation failure must remain represented.");
        Assert(message.Contains(
                "Listener cleanup could not be confirmed. Restart the host before retrying.",
                StringComparison.Ordinal),
            "A failed rollback must produce safe recovery guidance.");
        Assert(message.Contains(
                "An unexpected error occurred. Details are hidden to protect local paths and sensitive data.",
                StringComparison.Ordinal),
            "The secondary failure must use the safe error mapper.");
        Assert(!message.Contains(secretActivationDetail, StringComparison.Ordinal),
            "The activation exception detail must not leak.");
        Assert(!message.Contains(secretRollbackDetail, StringComparison.Ordinal),
            "The rollback exception detail must not leak.");
    }

    private static async Task DisposalDuringShutdownDoesNotReportASecondFailureAsync()
    {
        const string secretDisposalDetail = "disposed-secret-detail";
        var message = await ParticipationActivationRecovery.RecoverAsync(
            new InvalidOperationException("activation-private-detail"),
            () => Task.FromException(new ObjectDisposedException(secretDisposalDetail))).ConfigureAwait(false);

        Assert(!message.Contains("Restart the host before retrying.", StringComparison.Ordinal),
            "A disposed lifecycle during shutdown must not report a second cleanup failure.");
        Assert(!message.Contains(secretDisposalDetail, StringComparison.Ordinal),
            "The disposal exception detail must not leak.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
