using WhiteP.Host.Windows.Chat;

namespace WhiteP.Host.Windows.Tools;

/// <summary>
/// Executes a validated agent-tool call in a separately launched, protocol-limited worker process.
/// The implementation must never dispatch model-controlled strings as executable paths or shell commands.
/// </summary>
public interface IAgentToolProcessExecutor
{
    Task<AgentToolResult> ExecuteAsync(
        AgentToolCall call,
        CancellationToken cancellationToken = default);
}
