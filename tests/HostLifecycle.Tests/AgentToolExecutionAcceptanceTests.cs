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
        var readOnlyProcessExecutor = CreateCountingProcessExecutor();
        var readOnlyExecutor = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(new[] { readOnlyTool }),
            readOnlyConfirmation,
            readOnlyProcessExecutor);
        var readOnlyOutcome = await readOnlyExecutor.ExecuteAsync(CreateCall("get_host_status", "{}"));
        Assert(readOnlyOutcome.Status == AgentToolExecutionStatus.Executed &&
            readOnlyOutcome.Result?.Content == "Executed get_host_status.",
            "A valid read-only tool must execute and return its result.");
        Assert(readOnlyConfirmation.RequestCount == 0,
            "A read-only tool must not request side-effect confirmation.");
        Assert(readOnlyTool.ExecutionCount == 0 && readOnlyProcessExecutor.InvocationCount == 1,
            "A valid read-only tool must execute once in the worker process, never in the host process.");

        var sideEffectTool = new TestAgentTool("delete_model", requiresConfirmation: true);
        var approveConfirmation = new FakeConfirmationService((_, _, _) => Task.FromResult(true));
        var approveProcessExecutor = CreateCountingProcessExecutor();
        var approveExecutor = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(new[] { sideEffectTool }),
            approveConfirmation,
            approveProcessExecutor);
        var approved = await approveExecutor.ExecuteAsync(CreateCall("delete_model", "{}"));
        Assert(approved.Status == AgentToolExecutionStatus.Executed,
            "An explicitly approved side-effecting tool must execute.");
        Assert(approveConfirmation.RequestCount == 1 && sideEffectTool.ExecutionCount == 0 &&
            approveProcessExecutor.InvocationCount == 1,
            "An approved side-effecting tool must be confirmed once and dispatched only to the worker process.");

        var deniedTool = new TestAgentTool("delete_model", requiresConfirmation: true);
        var denyConfirmation = new FakeConfirmationService((_, _, _) => Task.FromResult(false));
        var denyProcessExecutor = CreateCountingProcessExecutor();
        var denyExecutor = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(new[] { deniedTool }),
            denyConfirmation,
            denyProcessExecutor);
        var denied = await denyExecutor.ExecuteAsync(CreateCall("delete_model", "{}"));
        Assert(denied.Status == AgentToolExecutionStatus.Declined && denied.Result is null,
            "A denied request must return a declined outcome without a result.");
        Assert(denyConfirmation.RequestCount == 1 && deniedTool.ExecutionCount == 0 &&
            denyProcessExecutor.InvocationCount == 0,
            "A denied side-effecting tool must never be dispatched to the worker.");

        var invalidTool = new TestAgentTool("delete_model", requiresConfirmation: true);
        var invalidConfirmation = new FakeConfirmationService((_, _, _) => Task.FromResult(true));
        var invalidProcessExecutor = CreateCountingProcessExecutor();
        var invalidExecutor = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(new[] { invalidTool }),
            invalidConfirmation,
            invalidProcessExecutor);
        await ExpectThrowsAsync<AgentToolArgumentsRejectedException>(
            () => invalidExecutor.ExecuteAsync(CreateCall("delete_model", """{"unexpected":true}""")));
        Assert(invalidConfirmation.RequestCount == 0 && invalidTool.ExecutionCount == 0 &&
            invalidProcessExecutor.InvocationCount == 0,
            "Invalid arguments must be rejected before prompting or worker execution.");

        var unknownConfirmation = new FakeConfirmationService((_, _, _) => Task.FromResult(true));
        var unknownProcessExecutor = CreateCountingProcessExecutor();
        var unknownExecutor = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(), unknownConfirmation, unknownProcessExecutor);
        await ExpectThrowsAsync<AgentToolNotAllowedException>(
            () => unknownExecutor.ExecuteAsync(CreateCall("delete_model", "{}")));
        Assert(unknownConfirmation.RequestCount == 0 && unknownProcessExecutor.InvocationCount == 0,
            "Unknown tools must be rejected before confirmation or worker launch.");

        using var preCancelled = new CancellationTokenSource();
        preCancelled.Cancel();
        var preCancelledTool = new TestAgentTool("delete_model", requiresConfirmation: true);
        var preCancelledConfirmation = new FakeConfirmationService((_, _, _) => Task.FromResult(true));
        var preCancelledProcessExecutor = CreateCountingProcessExecutor();
        var preCancelledExecutor = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(new[] { preCancelledTool }),
            preCancelledConfirmation,
            preCancelledProcessExecutor);
        await ExpectThrowsAsync<OperationCanceledException>(
            () => preCancelledExecutor.ExecuteAsync(CreateCall("delete_model", "{}"), preCancelled.Token));
        Assert(preCancelledConfirmation.RequestCount == 0 && preCancelledTool.ExecutionCount == 0 &&
            preCancelledProcessExecutor.InvocationCount == 0,
            "An already-cancelled request must not prompt or launch the worker.");

        using var cancelledAfterApproval = new CancellationTokenSource();
        var racingTool = new TestAgentTool("delete_model", requiresConfirmation: true);
        var racingConfirmation = new FakeConfirmationService((_, _, _) =>
        {
            cancelledAfterApproval.Cancel();
            return Task.FromResult(true);
        });
        var racingProcessExecutor = CreateCountingProcessExecutor();
        var racingExecutor = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(new[] { racingTool }),
            racingConfirmation,
            racingProcessExecutor);
        await ExpectThrowsAsync<OperationCanceledException>(
            () => racingExecutor.ExecuteAsync(CreateCall("delete_model", "{}"), cancelledAfterApproval.Token));
        Assert(racingConfirmation.RequestCount == 1 && racingTool.ExecutionCount == 0 &&
            racingProcessExecutor.InvocationCount == 0,
            "Cancellation racing with approval must still prevent worker execution.");

        var confirmationFailureTool = new TestAgentTool("delete_model", requiresConfirmation: true);
        var failedConfirmation = new FakeConfirmationService((_, _, _) =>
            Task.FromException<bool>(new InvalidOperationException("Confirmation UI failed.")));
        var failedProcessExecutor = CreateCountingProcessExecutor();
        var failedExecutor = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(new[] { confirmationFailureTool }),
            failedConfirmation,
            failedProcessExecutor);
        await ExpectThrowsAsync<InvalidOperationException>(
            () => failedExecutor.ExecuteAsync(CreateCall("delete_model", "{}")));
        Assert(confirmationFailureTool.ExecutionCount == 0 && failedProcessExecutor.InvocationCount == 0,
            "A failed confirmation service must fail closed and never launch a worker.");

        Console.WriteLine("PASS: side-effecting tools require explicit confirmation and are dispatched only to the worker after approval.");
    }

    internal static AgentToolProcessExecutor CreateProcessExecutor(TimeSpan? timeout = null)
    {
        var executable = Environment.ProcessPath
            ?? throw new InvalidOperationException("The lifecycle test executable path is unavailable.");
        var arguments = new List<string>();
        if (string.Equals(Path.GetFileNameWithoutExtension(executable), "dotnet", StringComparison.OrdinalIgnoreCase))
        {
            arguments.Add(typeof(AgentToolExecutionAcceptanceTests).Assembly.Location);
        }

        arguments.Add("--test-agent-tool-worker");
        return new AgentToolProcessExecutor(executable, arguments, timeout ?? TimeSpan.FromSeconds(15));
    }

    public static Task<int> RunWorkerAsync()
    {
        var registry = new AgentToolRegistry(new IAgentTool[]
        {
            new TestAgentTool("get_host_status", requiresConfirmation: false),
            new TestAgentTool("delete_model", requiresConfirmation: true),
            new TestAgentTool("hang_tool", requiresConfirmation: false)
        });
        return AgentToolWorkerHost.RunAsync(
            Console.OpenStandardInput(),
            Console.OpenStandardOutput(),
            registry);
    }

    private static CountingProcessExecutor CreateCountingProcessExecutor() =>
        new(CreateProcessExecutor());

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

    private sealed class CountingProcessExecutor(IAgentToolProcessExecutor inner) : IAgentToolProcessExecutor
    {
        public int InvocationCount { get; private set; }

        public Task<AgentToolResult> ExecuteAsync(AgentToolCall call, CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            return inner.ExecuteAsync(call, cancellationToken);
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

        public async Task<AgentToolResult> ExecuteAsync(JsonElement arguments, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ExecutionCount++;

            if (Definition.Name == "hang_tool")
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }

            if (Environment.GetEnvironmentVariable("WHITEP_AGENT_TOOL_SECRET_CANARY") is not null)
            {
                return new AgentToolResult("Worker inherited an unapproved environment variable.", IsError: true);
            }

            return new AgentToolResult($"Executed {Definition.Name}.");
        }
    }
}
