using System;
using System.Security.Cryptography;
using System.Text;
using WhiteP.Shared.Security;

namespace WhiteP.Host.Windows.Security;

/// <summary>
/// Uses Windows DPAPI with CurrentUser scope. This is OS-backed protection, not a
/// custom cryptographic primitive. It does not defend against malicious code already
/// executing as the same user.
/// </summary>
public sealed class WindowsDpapiSecretProtector : IUserSecretProtector
{
    private static readonly byte[] AdditionalEntropy = Encoding.UTF8.GetBytes("WhiteP.Host.Identity.v1");

    public byte[] Protect(byte[] plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Windows DPAPI is available only on Windows.");
        return ProtectedData.Protect(plaintext, AdditionalEntropy, DataProtectionScope.CurrentUser);
    }

    public byte[] Unprotect(byte[] protectedPayload)
    {
        ArgumentNullException.ThrowIfNull(protectedPayload);
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Windows DPAPI is available only on Windows.");
        return ProtectedData.Unprotect(protectedPayload, AdditionalEntropy, DataProtectionScope.CurrentUser);
    }
}
