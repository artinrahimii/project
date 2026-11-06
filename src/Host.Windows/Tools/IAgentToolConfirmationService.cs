using WhiteP.Host.Windows.Chat;

namespace WhiteP.Host.Windows.Tools;

/// <summary>
/// Requests explicit human approval for a proposed side-effecting tool call.
/// Implementations must fail closed when approval cannot be obtained.
/// </summary>
public interface IAgentToolConfirmationService
{
    Task<bool> ConfirmAsync(
        AgentToolDefinition definition,
        AgentToolCall call,
        CancellationToken cancellationToken);
}
