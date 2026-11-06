using System.Text.Json;

namespace WhiteP.Host.Windows.Chat;

/// <summary>
/// Untrusted tool-call data proposed by a model. Arguments are cloned so their
/// lifetime is independent of the JSON document that produced the call.
/// </summary>
public sealed class AgentToolCall
{
    public AgentToolCall(string name, JsonElement arguments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Arguments = arguments.Clone();
    }

    public string Name { get; }

    public JsonElement Arguments { get; }
}

/// <summary>
/// Tool metadata that may be advertised to a model only after the tool has been
/// explicitly registered in the host allowlist.
/// </summary>
public sealed record AgentToolDefinition(
    string Name,
    string Description,
    string ParametersJsonSchema,
    bool RequiresUserConfirmation = false);

/// <summary>Typed result returned by an approved tool executor.</summary>
public sealed record AgentToolResult(string Content, bool IsError = false);
