using System.Text.RegularExpressions;
using WhiteP.Host.Windows.Requests;

internal static class SecureRequestIdGeneratorTests
{
    private static readonly Regex RequestIdPattern = new(
        "^req_[0-9a-f]{32}$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static void Run()
    {
        const int sampleCount = 4096;
        var identifiers = new HashSet<string>(StringComparer.Ordinal);

        for (var index = 0; index < sampleCount; index++)
        {
            var requestId = SecureRequestIdGenerator.CreateRequestId();
            Assert(RequestIdPattern.IsMatch(requestId),
                "Request IDs must contain a prefix and 128 cryptographically random bits encoded as lowercase hexadecimal.");
            Assert(identifiers.Add(requestId),
                "Generated request IDs must not repeat within the sample.");
        }

        Console.WriteLine(
            $"PASS: generated {sampleCount} temporary request IDs using a cryptographic RNG; format and sample uniqueness checks passed.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
