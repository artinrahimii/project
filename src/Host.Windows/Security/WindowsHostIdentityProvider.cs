using System;
using System.IO;
using Nethermind.Libp2p.Core;
using WhiteP.Shared.Security;

namespace WhiteP.Host.Windows.Security;

/// <summary>
/// Provides a stable per-Windows-user host identity. The private key is DPAPI-protected
/// beneath LocalApplicationData and created only on first use. Existing corrupt files
/// fail closed so startup never silently changes the host PeerId.
/// </summary>
public sealed class WindowsHostIdentityProvider
{
    private readonly HostIdentityStore _store;

    public WindowsHostIdentityProvider(string? filePath = null)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("The Windows host identity provider requires Windows DPAPI.");

        string path;
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            path = Path.GetFullPath(filePath);
        }
        else
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(localAppData))
                throw new InvalidOperationException("The current user's LocalApplicationData directory is unavailable.");
            path = Path.Combine(localAppData, "WhiteP", "security", "host-identity-v1.dpapi");
        }

        _store = new HostIdentityStore(path, new WindowsDpapiSecretProtector());
    }

    public Identity LoadOrCreate() => _store.LoadOrCreate();
}
