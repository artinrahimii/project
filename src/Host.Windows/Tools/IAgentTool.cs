using System.Text.Json;
using WhiteP.Host.Windows.Chat;

namespace WhiteP.Host.Windows.Tools;

/// <summary>
/// Contract implemented only by trusted, host-owned tools. Implementations must
/// validate the complete argument object and must not perform work in
/// ValidateArguments. ExecuteAsync is invoked by the isolated worker process only;
/// the host-side coordinator dispatches through IAgentToolProcessExecutor after policy
/// validation and any required user confirmation.
/// </summary>
public interface IAgentTool
{
    AgentToolDefinition Definition { get; }

    /// <summary>Validates arguments without performing I/O or changing state.</summary>
    bool ValidateArguments(JsonElement arguments, out string? errorMessage);

    /// <summary>Executes this trusted tool after policy and confirmation checks.</summary>
    Task<AgentToolResult> ExecuteAsync(JsonElement arguments, CancellationToken cancellationToken);
}
