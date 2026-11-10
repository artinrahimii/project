using System.Security.Cryptography;
using System.Text;
using WhiteP.Host.Windows.Models.Catalog;

internal static class InstalledCatalogModelVerifierAcceptanceTests
{
    public static async Task RunAsync()
    {
        var directory = Path.Combine(Path.GetTempPath(), "whitep-installed-model-check-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var modelPath = Path.Combine(directory, "verified-model.gguf");
            var bytes = Encoding.ASCII.GetBytes("GGUF-test-model-content");
            await File.WriteAllBytesAsync(modelPath, bytes);
            var sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

            Assert(await InstalledCatalogModelVerifier.VerifyAsync("catalog/model-a", "catalog/model-a", modelPath, sha256),
                "An exact catalog/backend ID and matching installed-file SHA-256 must be accepted.");
            Assert(!await InstalledCatalogModelVerifier.VerifyAsync("catalog/model-a", "catalog/model-b", modelPath, sha256),
                "A backend model ID mismatch must be rejected.");
            Assert(!await InstalledCatalogModelVerifier.VerifyAsync("catalog/model-a", "catalog/model-a", modelPath, new string('0', 64)),
                "A file whose SHA-256 differs from the catalog-pinned checksum must be rejected.");
            Assert(!await InstalledCatalogModelVerifier.VerifyAsync("catalog/model-a", "catalog/model-a", Path.Combine(directory, "missing.gguf"), sha256),
                "A missing installed model file must be rejected.");
            Assert(!await InstalledCatalogModelVerifier.VerifyAsync("catalog/model-a", "catalog/model-a", modelPath, "not-a-sha256"),
                "Malformed checksum metadata must be rejected.");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }

        Console.WriteLine("PASS: installed catalog model verifier enforces exact active model ID, regular file and SHA-256 equality.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
