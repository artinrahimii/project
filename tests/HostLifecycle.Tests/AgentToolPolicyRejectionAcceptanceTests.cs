using System.Text;
using System.Text.Json;
using WhiteP.Host.Windows.Chat;
using WhiteP.Host.Windows.Tools;

internal static class AgentToolPolicyRejectionAcceptanceTests
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
        // Drive invalid calls directly through the real worker process so its independent
        // allowlist and argument validation are covered, not only the host-side preflight.
        var processExecutor = AgentToolExecutionAcceptanceTests.CreateProcessExecutor(TimeSpan.FromSeconds(10));
        await ExpectWorkerRejectedAsync(
            () => processExecutor.ExecuteAsync(CreateCall("unknown_tool", "{}")),
            "An unknown worker tool name must be rejected.");
        await ExpectWorkerRejectedAsync(
            () => processExecutor.ExecuteAsync(CreateCall("get_host_status", """{"unexpected":true}""")),
            "Invalid arguments must be rejected again inside the worker.");

        // The worker protocol rejects malformed request shapes and returns only a stable
        // non-sensitive error code; it must not execute any tool while doing so.
        var protocolTool = new TestAgentTool("get_host_status", requiresConfirmation: false);
        var protocolRegistry = new AgentToolRegistry(new[] { protocolTool });
        using var malformedInput = new MemoryStream(Encoding.UTF8.GetBytes("""{"name":null,"arguments":{}}"""));
        using var malformedOutput = new MemoryStream();
        var malformedExitCode = await AgentToolWorkerHost.RunAsync(malformedInput, malformedOutput, protocolRegistry);
        Assert(malformedExitCode == 2, "Malformed protocol fields must return the worker-rejected exit code.");
        using (var errorResponse = JsonDocument.Parse(malformedOutput.ToArray()))
        {
            Assert(errorResponse.RootElement.GetProperty("ok").GetBoolean() == false &&
                errorResponse.RootElement.GetProperty("errorCode").GetString() == "worker_rejected",
                "Malformed worker input must produce only the stable worker_rejected response.");
        }
        Assert(protocolTool.ExecutionCount == 0, "Malformed requests must not execute a registered tool.");

        // The coordinator is the policy gate for confirmation. Declining a side-effecting
        // operation must never launch the process worker.
        var deniedTool = new TestAgentTool("delete_model", requiresConfirmation: true);
        var deniedConfirmation = new FixedConfirmationService(approved: false);
        var deniedProcess = new CountingProcessExecutor(processExecutor);
        var deniedCoordinator = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(new[] { deniedTool }),
            deniedConfirmation,
            deniedProcess, new SupportedAgentToolCallingBackend());
        var denied = await deniedCoordinator.ExecuteAsync(CreateCall("delete_model", "{}"));
        Assert(denied.Status == AgentToolExecutionStatus.Declined && denied.Result is null,
            "A side-effecting operation without positive confirmation must return a declined result.");
        Assert(deniedConfirmation.RequestCount == 1 && deniedProcess.InvocationCount == 0 &&
            deniedTool.ExecutionCount == 0,
            "A declined operation must not launch the worker or execute tool code.");

        // Positive approval permits exactly one worker dispatch; the host-side tool object
        // must not execute directly in the parent process.
        var approvedTool = new TestAgentTool("delete_model", requiresConfirmation: true);
        var approvedConfirmation = new FixedConfirmationService(approved: true);
        var approvedProcess = new CountingProcessExecutor(processExecutor);
        var approvedCoordinator = new AgentToolExecutionCoordinator(
            new AgentToolRegistry(new[] { approvedTool }),
            approvedConfirmation,
            approvedProcess, new SupportedAgentToolCallingBackend());
        var approved = await approvedCoordinator.ExecuteAsync(CreateCall("delete_model", "{}"));
        Assert(approved.Status == AgentToolExecutionStatus.Executed &&
            approved.Result?.Content == "Executed delete_model.",
            "A confirmed side-effecting operation must reach the worker and return its validated result.");
        Assert(approvedConfirmation.RequestCount == 1 && approvedProcess.InvocationCount == 1 &&
            approvedTool.ExecutionCount == 0,
            "An approved operation must dispatch exactly once and stay out of the parent process.");

        Console.WriteLine("PASS: worker rejects unknown tools and invalid input; side-effecting calls never dispatch without confirmation.");
    }

    private static AgentToolCall CreateCall(string name, string json)
    {
        using var document = JsonDocument.Parse(json);
        return new AgentToolCall(name, document.RootElement);
    }

    private static async Task ExpectWorkerRejectedAsync(
        Func<Task<AgentToolResult>> execute,
        string assertionMessage)
    {
        try
        {
            _ = await execute();
        }
        catch (AgentToolWorkerFailedException)
        {
            return;
        }

        throw new InvalidOperationException(assertionMessage);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class FixedConfirmationService(bool approved) : IAgentToolConfirmationService
    {
        public int RequestCount { get; private set; }

        public Task<bool> ConfirmAsync(
            AgentToolDefinition definition,
            AgentToolCall call,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RequestCount++;
            return Task.FromResult(approved);
        }
    }

    private sealed class CountingProcessExecutor(IAgentToolProcessExecutor inner) : IAgentToolProcessExecutor
    {
        public int InvocationCount { get; private set; }

        public Task<AgentToolResult> ExecuteAsync(
            AgentToolCall call,
            CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            return inner.ExecuteAsync(call, cancellationToken);
        }
    }

    private sealed class TestAgentTool(string name, bool requiresConfirmation) : IAgentTool
    {
        public AgentToolDefinition Definition { get; } =
            new(name, "T0096 policy-rejection acceptance test tool.", EmptyObjectSchema, requiresConfirmation);

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
