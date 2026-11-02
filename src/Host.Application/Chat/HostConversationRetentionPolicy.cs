namespace WhiteP.Host.Application.Chat;

/// <summary>
/// Defines the default host-side retention boundary for conversation content.
/// The default keeps prompt and response text in the current process session only.
/// </summary>
public enum HostConversationRetentionScope
{
    ProcessSessionOnly
}

/// <summary>
/// Immutable host-side conversation retention policy.
/// This application policy does not claim control over operating-system paging,
/// crash dumps, external diagnostics, or the local inference engine process.
/// </summary>
public sealed class HostConversationRetentionPolicy
{
    private HostConversationRetentionPolicy()
    {
    }

    public static HostConversationRetentionPolicy Default { get; } = new();

    public HostConversationRetentionScope Scope => HostConversationRetentionScope.ProcessSessionOnly;

    public bool PersistsPromptAndResponse => false;
}
