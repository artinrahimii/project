using System.Collections.ObjectModel;
using System.Text.Json;
using WhiteP.Host.Application;
using WhiteP.Host.Windows.Chat;

namespace WhiteP.Host.Windows.Tools;

/// <summary>
/// Explicit allowlist for trusted, host-owned agent tools. It performs no
/// reflection-based discovery and registers no tools by default.
/// </summary>
public sealed class AgentToolRegistry
{
    private const int MaximumSchemaCharacters = 16_384;

    private static readonly HashSet<string> ReservedCommandNames = new(StringComparer.Ordinal)
    {
        "bash",
        "cmd",
        "execute_command",
        "execute_shell",
        "powershell",
        "process_exec",
        "run_command",
        "shell",
        "system_command",
        "terminal"
    };

    private readonly IReadOnlyDictionary<string, Registration> _registered;

    private readonly IReadOnlyList<AgentToolDefinition> _definitions;

    public AgentToolRegistry()
        : this(Array.Empty<IAgentTool>())
    {
    }

    public AgentToolRegistry(IEnumerable<IAgentTool> explicitlyAllowlistedTools)
    {
        ArgumentNullException.ThrowIfNull(explicitlyAllowlistedTools);

        var registrations = new Dictionary<string, Registration>(StringComparer.Ordinal);
        foreach (var tool in explicitlyAllowlistedTools)
        {
            if (tool is null)
            {
                throw new AgentToolConfigurationException();
            }

            var definition = tool.Definition;
            ValidateDefinition(definition);

            if (!registrations.TryAdd(definition.Name, new Registration(tool, definition)))
            {
                throw new AgentToolConfigurationException();
            }
        }

        _registered = new ReadOnlyDictionary<string, Registration>(registrations);
        _definitions = Array.AsReadOnly(
            registrations.Values
                .Select(registration => registration.Definition)
                .OrderBy(definition => definition.Name, StringComparer.Ordinal)
                .ToArray());
    }

    /// <summary>Returns model-advertisable definitions only after fresh backend/model/protocol capability is verified.</summary>
    public async Task<IReadOnlyList<AgentToolDefinition>> GetModelAdvertisableDefinitionsAsync(IInferenceBackend backend, CancellationToken cancellationToken = default)
    {
        var capability = await AgentToolCapabilityPolicy.EvaluateAsync(backend, cancellationToken).ConfigureAwait(false);
        return capability.IsSupported ? _definitions : Array.Empty<AgentToolDefinition>();
    }

    public bool IsAllowlisted(string name) =>
        name is not null && _registered.ContainsKey(name);

    public AgentToolDefinition GetDefinition(string name) => Find(name).Definition;

    public IAgentTool Resolve(string name) => Find(name).Tool;

    public IAgentTool Resolve(AgentToolCall call)
    {
        ArgumentNullException.ThrowIfNull(call);
        return Resolve(call.Name);
    }

    /// <summary>
    /// Resolves a model-proposed call and applies tool-specific validation before
    /// returning its implementation. This method never executes the tool.
    /// </summary>
    public IAgentTool ResolveValidated(AgentToolCall call)
    {
        ArgumentNullException.ThrowIfNull(call);
        var registration = Find(call.Name);
        if (call.Arguments.ValueKind != JsonValueKind.Object ||
            !registration.Tool.ValidateArguments(call.Arguments, out _))
        {
            throw new AgentToolArgumentsRejectedException();
        }

        return registration.Tool;
    }

    private Registration Find(string? name)
    {
        if (name is null || !_registered.TryGetValue(name, out var registration))
        {
            throw new AgentToolNotAllowedException();
        }

        return registration;
    }

    private static void ValidateDefinition(AgentToolDefinition? definition)
    {
        if (definition is null ||
            !IsValidName(definition.Name) ||
            ReservedCommandNames.Contains(definition.Name) ||
            string.IsNullOrWhiteSpace(definition.Description) ||
            definition.Description.Length > 300 ||
            definition.Description.Any(char.IsControl))
        {
            throw new AgentToolConfigurationException();
        }

        ValidateParameterSchema(definition.ParametersJsonSchema);
    }

    private static bool IsValidName(string? name)
    {
        if (string.IsNullOrEmpty(name) || name.Length > 64 || name[0] is < 'a' or > 'z')
        {
            return false;
        }

        for (var index = 1; index < name.Length; index++)
        {
            var character = name[index];
            if ((character < 'a' || character > 'z') &&
                (character < '0' || character > '9') &&
                character != '_')
            {
                return false;
            }
        }

        return true;
    }

    private static void ValidateParameterSchema(string? schema)
    {
        if (string.IsNullOrWhiteSpace(schema) || schema.Length > MaximumSchemaCharacters)
        {
            throw new AgentToolConfigurationException();
        }

        try
        {
            using var document = JsonDocument.Parse(
                schema,
                new JsonDocumentOptions { MaxDepth = 16 });
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("type", out var type) ||
                type.ValueKind != JsonValueKind.String ||
                !string.Equals(type.GetString(), "object", StringComparison.Ordinal) ||
                !root.TryGetProperty("properties", out var properties) ||
                properties.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("additionalProperties", out var additionalProperties) ||
                additionalProperties.ValueKind != JsonValueKind.False)
            {
                throw new AgentToolConfigurationException();
            }

            foreach (var property in properties.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.Object)
                {
                    throw new AgentToolConfigurationException();
                }
            }

            if (root.TryGetProperty("required", out var required))
            {
                if (required.ValueKind != JsonValueKind.Array)
                {
                    throw new AgentToolConfigurationException();
                }

                foreach (var item in required.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.String ||
                        !properties.TryGetProperty(item.GetString()!, out _))
                    {
                        throw new AgentToolConfigurationException();
                    }
                }
            }
        }
        catch (JsonException)
        {
            throw new AgentToolConfigurationException();
        }
    }

    private sealed record Registration(IAgentTool Tool, AgentToolDefinition Definition);
}

public sealed class AgentToolNotAllowedException : InvalidOperationException
{
    public AgentToolNotAllowedException()
        : base("The requested agent tool is not allowlisted.")
    {
    }
}

public sealed class AgentToolArgumentsRejectedException : InvalidOperationException
{
    public AgentToolArgumentsRejectedException()
        : base("The agent tool arguments are invalid.")
    {
    }
}

public sealed class AgentToolConfigurationException : InvalidOperationException
{
    public AgentToolConfigurationException()
        : base("The trusted agent-tool allowlist configuration is invalid.")
    {
    }
}
