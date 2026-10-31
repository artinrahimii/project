using System.Windows;
using System.Windows.Threading;
using WhiteP.Host.Windows.Chat;

namespace WhiteP.Host.Windows.Tools;

/// <summary>
/// Presents an owned, default-No WPF confirmation dialog for side-effecting tools.
/// Tool arguments are shown verbatim; oversized payloads are denied rather than
/// asking the user to approve arguments that are not all visible.
/// </summary>
public sealed class WpfAgentToolConfirmationService : IAgentToolConfirmationService
{
    private const int MaximumVisibleArgumentCharacters = 4_096;
    private readonly Window _owner;

    public WpfAgentToolConfirmationService(Window owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public Task<bool> ConfirmAsync(
        AgentToolDefinition definition,
        AgentToolCall call,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(call);
        cancellationToken.ThrowIfCancellationRequested();

        if (_owner.Dispatcher.CheckAccess())
        {
            return Task.FromResult(ShowConfirmation(definition, call, cancellationToken));
        }

        return _owner.Dispatcher.InvokeAsync(
            () => ShowConfirmation(definition, call, cancellationToken),
            DispatcherPriority.Normal,
            cancellationToken).Task;
    }

    private bool ShowConfirmation(
        AgentToolDefinition definition,
        AgentToolCall call,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var arguments = call.Arguments.GetRawText();

        if (arguments.Length > MaximumVisibleArgumentCharacters)
        {
            MessageBox.Show(
                _owner,
                $"The arguments for '{definition.Name}' exceed the safe display limit of " +
                $"{MaximumVisibleArgumentCharacters:N0} characters. The operation was not run.",
                "Agent tool not executed",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        var prompt =
            "This tool is marked as requiring confirmation because it may change data or " +
            "cause other side effects. Review the exact tool and arguments before approving.\n\n" +
            $"Tool: {definition.Name}\n" +
            $"Description: {definition.Description}\n\n" +
            $"Arguments (JSON):\n{arguments}\n\n" +
            "Choose Yes to allow this request once, or No to leave it unexecuted.";

        var answer = MessageBox.Show(
            _owner,
            prompt,
            "Confirm agent tool operation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        // If cancellation races with the modal dialog, a click on Yes is not enough.
        cancellationToken.ThrowIfCancellationRequested();
        return answer == MessageBoxResult.Yes;
    }
}
