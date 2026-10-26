using System.Text.Json;
using WhiteP.Host.Windows.Chat;

namespace WhiteP.Host.Windows.Tools;

/// <summary>
/// Contract implemented only by trusted, host-owned tools. Implementations must
/// validate the complete argument object and must not perform work in
/// ValidateArguments. Execution and side-effect confirmation are handled by
/// later policy components, not by the model.
/// </summary>
public interface IAgentTool
{
    AgentToolDefinition Definition { get; }

    /// <summary>Validates arguments without performing I/O or changing state.</summary>
    bool ValidateArguments(JsonElement arguments, out string? errorMessage);

    /// <summary>Executes this trusted tool after policy and confirmation checks.</summary>
    Task<AgentToolResult> ExecuteAsync(JsonElement arguments, CancellationToken cancellationToken);
}
