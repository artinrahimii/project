using System.Security.Cryptography;

namespace WhiteP.Host.Windows.Requests;

/// <summary>
/// Creates cryptographically random, temporary request identifiers.
/// Each identifier contains 128 random bits and carries no account/device identity.
/// </summary>
public static class SecureRequestIdGenerator
{
    private const int RandomByteCount = 16;
    private const string Prefix = "req_";

    public static string CreateRequestId()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(RandomByteCount);
        return Prefix + Convert.ToHexString(randomBytes).ToLowerInvariant();
    }
}
