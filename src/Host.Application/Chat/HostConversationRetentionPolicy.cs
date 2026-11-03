namespace WhiteP.Host.Application.Chat;

/// <summary>
/// Defines the host-side retention boundary for conversation content.
/// The default is process-session-only; durable local retention requires explicit
/// consent from the owner of the Windows computer.
/// </summary>
public enum HostConversationRetentionScope
{
    ProcessSessionOnly,
    LocalDeviceWithOwnerConsent
}

/// <summary>
/// Immutable host-side conversation retention policy. This policy does not claim control
/// over operating-system paging, crash dumps, external diagnostics, backups or child processes.
/// </summary>
public sealed class HostConversationRetentionPolicy
{
    private HostConversationRetentionPolicy(HostConversationRetentionScope scope, bool persistsPromptAndResponse)
    {
        Scope = scope;
        PersistsPromptAndResponse = persistsPromptAndResponse;
    }

    public static HostConversationRetentionPolicy Default { get; } =
        new(HostConversationRetentionScope.ProcessSessionOnly, persistsPromptAndResponse: false);

    public static HostConversationRetentionPolicy OwnerConsentedLocalDevice { get; } =
        new(HostConversationRetentionScope.LocalDeviceWithOwnerConsent, persistsPromptAndResponse: true);

    public HostConversationRetentionScope Scope { get; }

    public bool PersistsPromptAndResponse { get; }

    public static HostConversationRetentionPolicy FromOwnerConsent(bool ownerConsent) =>
        ownerConsent ? OwnerConsentedLocalDevice : Default;
}
