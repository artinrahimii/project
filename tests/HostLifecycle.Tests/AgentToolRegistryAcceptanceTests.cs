using System.Text.Json;
using WhiteP.Host.Windows.Chat;
using WhiteP.Host.Windows.Tools;

internal static class AgentToolRegistryAcceptanceTests
{
    private const string EmptyObjectSchema = """
        {
          "type": "object",
          "properties": {},
          "required": [],
          "additionalProperties": false
        }
        """;

    public static void Run()
    {
        var disabledByDefault = new AgentToolRegistry();
        Assert(disabledByDefault.Definitions.Count == 0,
            "No agent tool may be available before an explicit allowlist is configured.");
        Assert(!disabledByDefault.IsAllowlisted("run_command"),
            "Arbitrary system command tools must not be enabled by default.");
        ExpectNotAllowed(() => disabledByDefault.Resolve("run_command"));
        ExpectNotAllowed(() => disabledByDefault.Resolve("get_host_status"));

        var readOnlyTool = new TestAgentTool("get_host_status", requiresConfirmation: false);
        var registry = new AgentToolRegistry(new[] { readOnlyTool });
        Assert(registry.Definitions.Count == 1 && registry.Definitions[0].Name == "get_host_status",
            "Only explicitly allowlisted tool metadata may be advertised.");
        Assert(registry.IsAllowlisted("get_host_status"),
            "The explicit allowlist must expose its registered tool.");
        Assert(ReferenceEquals(registry.Resolve("get_host_status"), readOnlyTool),
            "Resolution must return only the explicitly registered implementation.");
        Assert(registry.GetDefinition("get_host_status").ParametersJsonSchema == EmptyObjectSchema,
            "The registry must preserve the declared argument schema.");
        ExpectNotAllowed(() => registry.Resolve("unknown_tool"));
        ExpectNotAllowed(() => registry.Resolve("Get_Host_Status"));

        var validCall = CreateCall("get_host_status", "{}");
        Assert(ReferenceEquals(registry.ResolveValidated(validCall), readOnlyTool),
            "A valid call to an allowlisted tool must resolve successfully.");

        var invalidCall = CreateCall("get_host_status", """{"unexpected":true}""");
        ExpectArgumentsRejected(() => registry.ResolveValidated(invalidCall));

        ExpectConfigurationFailure(() => new AgentToolRegistry(new[] { readOnlyTool, readOnlyTool }));
        ExpectConfigurationFailure(() => new AgentToolRegistry(
            new[] { new TestAgentTool("run_command", requiresConfirmation: false) }));
        ExpectConfigurationFailure(() => new AgentToolRegistry(
            new[] { new TestAgentTool("unsafe_schema", requiresConfirmation: false,
                schema: """{"type":"object","properties":{},"required":[],"additionalProperties":true}""") }));
        ExpectConfigurationFailure(() => new AgentToolRegistry(
            new[] { new TestAgentTool("bad_schema", requiresConfirmation: false, schema: "not-json") }));

        var sideEffectTool = new TestAgentTool("delete_model", requiresConfirmation: true);
        var sideEffectRegistry = new AgentToolRegistry(new[] { sideEffectTool });
        Assert(sideEffectRegistry.GetDefinition("delete_model").RequiresUserConfirmation,
            "The tool contract must explicitly mark side-effect tools as requiring confirmation.");

        Console.WriteLine("PASS: agent tool definitions are explicit, closed-schema allowlisted, and disabled by default.");
    }

    private static AgentToolCall CreateCall(string name, string json)
    {
        using var document = JsonDocument.Parse(json);
        return new AgentToolCall(name, document.RootElement);
    }

    private static void ExpectNotAllowed(Action action)
    {
        try
        {
            action();
        }
        catch (AgentToolNotAllowedException)
        {
            return;
        }

        throw new InvalidOperationException("An unknown or non-allowlisted tool was accepted.");
    }

    private static void ExpectArgumentsRejected(Action action)
    {
        try
        {
            action();
        }
        catch (AgentToolArgumentsRejectedException)
        {
            return;
        }

        throw new InvalidOperationException("Invalid tool arguments were accepted.");
    }

    private static void ExpectConfigurationFailure(Action action)
    {
        try
        {
            action();
        }
        catch (AgentToolConfigurationException)
        {
            return;
        }

        throw new InvalidOperationException("An unsafe allowlist configuration was accepted.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class TestAgentTool : IAgentTool
    {
        public TestAgentTool(string name, bool requiresConfirmation, string? schema = null)
        {
            Definition = new AgentToolDefinition(
                name,
                "Test-only tool registration.",
                schema ?? EmptyObjectSchema,
                requiresConfirmation);
        }

        public AgentToolDefinition Definition { get; }

        public bool ValidateArguments(JsonElement arguments, out string? errorMessage)
        {
            var valid = arguments.ValueKind == JsonValueKind.Object && !arguments.EnumerateObject().Any();
            errorMessage = valid ? null : "The test tool accepts an empty object only.";
            return valid;
        }

        public Task<AgentToolResult> ExecuteAsync(JsonElement arguments, CancellationToken cancellationToken) =>
            Task.FromResult(new AgentToolResult("Test-only result."));
    }
}
