using System;
using System.Threading.Tasks;

namespace WhiteP.Host.Application;

/// <summary>
/// Keeps participation-activation rollback behavior executable and independently testable.
/// Error text is mapped through the host's safe user-message boundary.
/// </summary>
public static class ParticipationActivationRecovery
{
    private const string CleanupWarning =
        " Listener cleanup could not be confirmed. Restart the host before retrying. ";

    public static async Task<string> RecoverAsync(
        Exception activationException,
        Func<Task> deactivateAsync)
    {
        ArgumentNullException.ThrowIfNull(activationException);
        ArgumentNullException.ThrowIfNull(deactivateAsync);

        var cleanupFailureMessage = string.Empty;
        try
        {
            await deactivateAsync().ConfigureAwait(false);
        }
        catch (ObjectDisposedException)
        {
            // Window shutdown already owns final disposal.
        }
        catch (Exception rollbackException)
        {
            // Do not let a second failure escape the WPF async-void event handler.
            cleanupFailureMessage =
                CleanupWarning + HostErrorPresentation.ToUserMessage(rollbackException);
        }

        return
            $"Could not activate network participation: {HostErrorPresentation.ToUserMessage(activationException)}" +
            cleanupFailureMessage;
    }
}
