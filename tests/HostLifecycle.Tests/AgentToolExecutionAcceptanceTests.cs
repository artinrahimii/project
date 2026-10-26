using System.Text.Json;
using WhiteP.Host.Windows.Chat;
using WhiteP.Host.Windows.Tools;

internal static class AgentToolExecutionAcceptanceTests
{
    private const string EmptyObjectSchema = """
        {
          "type": "object",
          "properties": {},
          "required": [],
          "additionalProperties": false
        }
        """;

    public static async Task RunAsync()
    {
        var readOnlyTool = new TestAgentTool("get_host_status", requiresConfirmation: false);
        var readOnlyConfirmation = new FakeConfirmationService((_, _, _) => Task.FromResult(true));
        var readOnlyExecutor = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(new[] { readOnlyTool }),
            readOnlyConfirmation);
        var readOnlyOutcome = await readOnlyExecutor.ExecuteAsync(CreateCall("get_host_status", "{}"));
        Assert(readOnlyOutcome.Status == AgentToolExecutionStatus.Executed &&
            readOnlyOutcome.Result?.Content == "Executed get_host_status.",
            "A valid read-only tool must execute and return its result.");
        Assert(readOnlyConfirmation.RequestCount == 0,
            "A read-only tool must not request side-effect confirmation.");
        Assert(readOnlyTool.ExecutionCount == 1,
            "A valid read-only tool must execute exactly once.");

        var sideEffectTool = new TestAgentTool("delete_model", requiresConfirmation: true);
        var approveConfirmation = new FakeConfirmationService((_, _, _) => Task.FromResult(true));
        var approveExecutor = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(new[] { sideEffectTool }),
            approveConfirmation);
        var approved = await approveExecutor.ExecuteAsync(CreateCall("delete_model", "{}"));
        Assert(approved.Status == AgentToolExecutionStatus.Executed,
            "An explicitly approved side-effecting tool must execute.");
        Assert(approveConfirmation.RequestCount == 1 && sideEffectTool.ExecutionCount == 1,
            "A side-effecting tool must be confirmed exactly once before execution.");

        var deniedTool = new TestAgentTool("delete_model", requiresConfirmation: true);
        var denyConfirmation = new FakeConfirmationService((_, _, _) => Task.FromResult(false));
        var denyExecutor = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(new[] { deniedTool }),
            denyConfirmation);
        var denied = await denyExecutor.ExecuteAsync(CreateCall("delete_model", "{}"));
        Assert(denied.Status == AgentToolExecutionStatus.Declined && denied.Result is null,
            "A denied request must return a declined outcome without a result.");
        Assert(denyConfirmation.RequestCount == 1 && deniedTool.ExecutionCount == 0,
            "A denied side-effecting tool must never be invoked.");

        var invalidTool = new TestAgentTool("delete_model", requiresConfirmation: true);
        var invalidConfirmation = new FakeConfirmationService((_, _, _) => Task.FromResult(true));
        var invalidExecutor = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(new[] { invalidTool }),
            invalidConfirmation);
        await ExpectThrowsAsync<AgentToolArgumentsRejectedException>(
            () => invalidExecutor.ExecuteAsync(CreateCall("delete_model", """{"unexpected":true}""")));
        Assert(invalidConfirmation.RequestCount == 0 && invalidTool.ExecutionCount == 0,
            "Invalid arguments must be rejected before prompting or executing.");

        var unknownConfirmation = new FakeConfirmationService((_, _, _) => Task.FromResult(true));
        var unknownExecutor = new AgentToolExecutionCoordinator(new AgentToolRegistry(), unknownConfirmation);
        await ExpectThrowsAsync<AgentToolNotAllowedException>(
            () => unknownExecutor.ExecuteAsync(CreateCall("delete_model", "{}")));
        Assert(unknownConfirmation.RequestCount == 0,
            "Unknown tools must be rejected before user confirmation.");

        using var preCancelled = new CancellationTokenSource();
        preCancelled.Cancel();
        var preCancelledTool = new TestAgentTool("delete_model", requiresConfirmation: true);
        var preCancelledConfirmation = new FakeConfirmationService((_, _, _) => Task.FromResult(true));
        var preCancelledExecutor = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(new[] { preCancelledTool }),
            preCancelledConfirmation);
        await ExpectThrowsAsync<OperationCanceledException>(
            () => preCancelledExecutor.ExecuteAsync(CreateCall("delete_model", "{}"), preCancelled.Token));
        Assert(preCancelledConfirmation.RequestCount == 0 && preCancelledTool.ExecutionCount == 0,
            "An already-cancelled request must not prompt or execute.");

        using var cancelledAfterApproval = new CancellationTokenSource();
        var racingTool = new TestAgentTool("delete_model", requiresConfirmation: true);
        var racingConfirmation = new FakeConfirmationService((_, _, _) =>
        {
            cancelledAfterApproval.Cancel();
            return Task.FromResult(true);
        });
        var racingExecutor = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(new[] { racingTool }),
            racingConfirmation);
        await ExpectThrowsAsync<OperationCanceledException>(
            () => racingExecutor.ExecuteAsync(CreateCall("delete_model", "{}"), cancelledAfterApproval.Token));
        Assert(racingConfirmation.RequestCount == 1 && racingTool.ExecutionCount == 0,
            "Cancellation racing with approval must still prevent tool invocation.");

        var confirmationFailureTool = new TestAgentTool("delete_model", requiresConfirmation: true);
        var failedConfirmation = new FakeConfirmationService((_, _, _) =>
            Task.FromException<bool>(new InvalidOperationException("Confirmation UI failed.")));
        var failedExecutor = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(new[] { confirmationFailureTool }),
            failedConfirmation);
        await ExpectThrowsAsync<InvalidOperationException>(
            () => failedExecutor.ExecuteAsync(CreateCall("delete_model", "{}")));
        Assert(confirmationFailureTool.ExecutionCount == 0,
            "A failed confirmation service must fail closed and leave the tool unexecuted.");

        Console.WriteLine("PASS: side-effecting agent tools require explicit confirmation; decline, invalid calls and cancellation never execute.");
    }

    private static AgentToolCall CreateCall(string name, string json)
    {
        using var document = JsonDocument.Parse(json);
        return new AgentToolCall(name, document.RootElement);
    }

    private static async Task ExpectThrowsAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException($"Expected exception {typeof(TException).Name} was not thrown.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class FakeConfirmationService(
        Func<AgentToolDefinition, AgentToolCall, CancellationToken, Task<bool>> response) :
        IAgentToolConfirmationService
    {
        public int RequestCount { get; private set; }

        public Task<bool> ConfirmAsync(
            AgentToolDefinition definition,
            AgentToolCall call,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            return response(definition, call, cancellationToken);
        }
    }

    private sealed class TestAgentTool : IAgentTool
    {
        public TestAgentTool(string name, bool requiresConfirmation)
        {
            Definition = new AgentToolDefinition(
                name,
                "Acceptance-test tool.",
                EmptyObjectSchema,
                requiresConfirmation);
        }

        public AgentToolDefinition Definition { get; }

        public int ExecutionCount { get; private set; }

        public bool ValidateArguments(JsonElement arguments, out string? errorMessage)
        {
            var valid = arguments.ValueKind == JsonValueKind.Object && !arguments.EnumerateObject().Any();
            errorMessage = valid ? null : "The test tool accepts an empty object only.";
            return valid;
        }

        public Task<AgentToolResult> ExecuteAsync(JsonElement arguments, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ExecutionCount++;
            return Task.FromResult(new AgentToolResult($"Executed {Definition.Name}."));
        }
    }
}
