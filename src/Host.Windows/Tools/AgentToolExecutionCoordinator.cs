using WhiteP.Host.Windows.Chat;

namespace WhiteP.Host.Windows.Tools;

public enum AgentToolExecutionStatus
{
    Executed,
    Declined
}

/// <summary>
/// Outcome of an allowlisted agent-tool request. A declined request has no tool result
/// because the trusted implementation was not invoked.
/// </summary>
public sealed record AgentToolExecutionOutcome(
    AgentToolExecutionStatus Status,
    AgentToolResult? Result)
{
    public static AgentToolExecutionOutcome DeclinedByUser { get; } =
        new(AgentToolExecutionStatus.Declined, null);

    public static AgentToolExecutionOutcome Completed(AgentToolResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new AgentToolExecutionOutcome(AgentToolExecutionStatus.Executed, result);
    }
}

/// <summary>
/// The single policy-aware dispatch path for trusted agent tools. It validates the
/// allowlist entry and arguments before prompting, and never invokes a tool that
/// requires confirmation unless the confirmation service explicitly approves it.
/// Process isolation and execution time limits are supplied by the subsequent tool
/// execution boundary and are intentionally not implemented by this coordinator.
/// </summary>
public sealed class AgentToolExecutionCoordinator
{
    private readonly AgentToolRegistry _registry;
    private readonly IAgentToolConfirmationService _confirmationService;

    public AgentToolExecutionCoordinator(
        AgentToolRegistry registry,
        IAgentToolConfirmationService confirmationService)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _confirmationService = confirmationService
            ?? throw new ArgumentNullException(nameof(confirmationService));
    }

    public async Task<AgentToolExecutionOutcome> ExecuteAsync(
        AgentToolCall call,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(call);
        cancellationToken.ThrowIfCancellationRequested();

        // Resolve the explicit allowlist and validate arguments before showing any prompt.
        var tool = _registry.ResolveValidated(call);
        var definition = _registry.GetDefinition(call.Name);

        if (definition.RequiresUserConfirmation)
        {
            var approved = await _confirmationService
                .ConfirmAsync(definition, call, cancellationToken)
                .ConfigureAwait(false);

            if (!approved)
            {
                return AgentToolExecutionOutcome.DeclinedByUser;
            }
        }

        // A cancellation that races with a positive click must still prevent invocation.
        cancellationToken.ThrowIfCancellationRequested();

        var result = await tool.ExecuteAsync(call.Arguments.Clone(), cancellationToken)
            .ConfigureAwait(false);

        return AgentToolExecutionOutcome.Completed(result);
    }
}
