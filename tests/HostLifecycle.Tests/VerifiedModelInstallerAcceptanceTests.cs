using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using WhiteP.Host.Windows.Models;
using WhiteP.Host.Windows.Models.Catalog;

internal static class VerifiedModelInstallerAcceptanceTests
{
    public static void Run()
    {
        InstallsOnlyAfterSha256Matches();
        UpdatesExistingModelAtomicallyAfterValidation();
        RejectsFailedUpdateAndPreservesPreviousModel();
        RejectsMissingUpdateTargetBeforeNetworkAccess();
        RefusesConcurrentDestinationChangeDuringUpdate();
        RejectsChecksumMismatchAndDeletesStagingFile();
        RejectsUnsupportedChecksumBeforeNetworkAccess();
        PreservesExistingDestinationWithoutDownloading();
        RejectsInvalidGgufMagicAfterChecksumMatch();
        RejectsUnsupportedGgufVersionAfterChecksumMatch();
        RejectsUnsupportedDeclaredFormatBeforeNetworkAccess();
        Console.WriteLine("PASS: model installation stages downloads, verifies SHA-256, and preserves final-path integrity.");
    }

    private static void InstallsOnlyAfterSha256Matches()
    {
        var bytes = CreateModelBytes();
        var expectedHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var handler = CreateHandler(bytes);
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader);
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "model.gguf");

        try
        {
            var result = installer.DownloadAndInstallAsync(
                CreateModel(bytes, expectedHash),
                destination).GetAwaiter().GetResult();

            Assert(File.Exists(destination), "A valid download should be promoted to the final model path.");
            Assert(result.InstalledPath == destination, "The installer should report the final path.");
            Assert(result.SizeBytes == bytes.LongLength, "The result should report the number of downloaded bytes.");
            Assert(result.Sha256 == expectedHash, "The result should report the verified SHA-256 checksum.");
            Assert(File.ReadAllBytes(destination).SequenceEqual(bytes), "The installed file must match the downloaded bytes.");
            Assert(Directory.GetFiles(folder).SequenceEqual(new[] { destination }),
                "Successful installation must leave no staging file behind.");
            Assert(handler.RequestCount == 1, "A valid model should be downloaded exactly once.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void RejectsChecksumMismatchAndDeletesStagingFile()
    {
        var bytes = CreateModelBytes();
        var actualHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var mismatchedHash = (actualHash[0] == '0' ? "1" : "0") + actualHash[1..];
        var handler = CreateHandler(bytes);
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader);
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "model.gguf");

        try
        {
            ExpectInvalidData(
                () => installer.DownloadAndInstallAsync(
                    CreateModel(bytes, mismatchedHash),
                    destination).GetAwaiter().GetResult(),
                "A checksum mismatch must not be installed.");

            Assert(!File.Exists(destination), "A checksum mismatch must not create the final model.");
            Assert(Directory.GetFiles(folder).Length == 0, "A checksum mismatch must remove the staging file.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void RejectsUnsupportedChecksumBeforeNetworkAccess()
    {
        var bytes = CreateModelBytes();
        var handler = CreateHandler(bytes);
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader);
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "model.gguf");

        try
        {
            ExpectInvalidData(
                () => installer.DownloadAndInstallAsync(
                    CreateModel(bytes, new string('a', 64), "md5"),
                    destination).GetAwaiter().GetResult(),
                "Unsupported checksum algorithms must be rejected.");

            Assert(handler.RequestCount == 0, "Unsupported checksum metadata must be rejected before downloading.");
            Assert(Directory.GetFiles(folder).Length == 0, "Rejected metadata must not create staging or final files.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void PreservesExistingDestinationWithoutDownloading()
    {
        var bytes = CreateModelBytes();
        var handler = CreateHandler(bytes);
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader);
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "model.gguf");
        var original = new byte[] { 91, 92, 93 };

        try
        {
            File.WriteAllBytes(destination, original);
            try
            {
                _ = installer.DownloadAndInstallAsync(
                    CreateModel(bytes, Convert.ToHexString(SHA256.HashData(bytes))),
                    destination).GetAwaiter().GetResult();
            }
            catch (IOException)
            {
                Assert(File.ReadAllBytes(destination).SequenceEqual(original),
                    "A pre-existing destination must remain unchanged.");
                Assert(handler.RequestCount == 0, "An existing destination should be detected before network access.");
                Assert(Directory.GetFiles(folder).SequenceEqual(new[] { destination }),
                    "An existing destination must not cause a staging file to be left behind.");
                return;
            }

            throw new InvalidOperationException("The installer must refuse to overwrite an existing model.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void RejectsInvalidGgufMagicAfterChecksumMatch()
    {
        var bytes = CreateModelBytes();
        bytes[0] = (byte)'B';
        var checksum = Convert.ToHexString(SHA256.HashData(bytes));
        var handler = CreateHandler(bytes);
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader);
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "invalid-magic.gguf");

        try
        {
            ExpectInvalidData(
                () => installer.DownloadAndInstallAsync(CreateModel(bytes, checksum), destination).GetAwaiter().GetResult(),
                "A checksum-correct file with invalid GGUF magic must not be installed.");
            Assert(handler.RequestCount == 1, "The checksum-correct but malformed payload should be downloaded before byte-format verification.");
            Assert(!File.Exists(destination), "Invalid GGUF magic must not reach the final path.");
            Assert(Directory.GetFiles(folder).Length == 0, "The malformed staging file must be removed.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void RejectsUnsupportedGgufVersionAfterChecksumMatch()
    {
        var bytes = CreateModelBytes();
        bytes[4] = 99;
        bytes[5] = 0;
        bytes[6] = 0;
        bytes[7] = 0;
        var checksum = Convert.ToHexString(SHA256.HashData(bytes));
        var handler = CreateHandler(bytes);
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader);
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "unsupported-version.gguf");

        try
        {
            ExpectInvalidData(
                () => installer.DownloadAndInstallAsync(CreateModel(bytes, checksum), destination).GetAwaiter().GetResult(),
                "An unsupported GGUF header version must not be installed.");
            Assert(!File.Exists(destination), "An unsupported GGUF version must not reach the final path.");
            Assert(Directory.GetFiles(folder).Length == 0, "The unsupported-version staging file must be removed.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void RejectsUnsupportedDeclaredFormatBeforeNetworkAccess()
    {
        var bytes = CreateModelBytes();
        var checksum = Convert.ToHexString(SHA256.HashData(bytes));
        var handler = CreateHandler(bytes);
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader);
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "wrong-format.gguf");

        try
        {
            ExpectInvalidData(
                () => installer.DownloadAndInstallAsync(CreateModel(bytes, checksum, format: "ONNX"), destination).GetAwaiter().GetResult(),
                "The installer should reject a non-GGUF declaration.");
            Assert(handler.RequestCount == 0, "An unsupported declared format must be rejected before network access.");
            Assert(Directory.GetFiles(folder).Length == 0, "An unsupported declaration must not create a staging file.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void UpdatesExistingModelAtomicallyAfterValidation()
    {
        var newBytes = CreateModelBytes();
        newBytes[16] ^= 0x51;
        var newHash = Convert.ToHexString(SHA256.HashData(newBytes)).ToLowerInvariant();
        var oldBytes = CreateModelBytes();
        oldBytes[16] ^= 0x27;
        var handler = CreateHandler(newBytes);
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader);
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "model.gguf");

        try
        {
            File.WriteAllBytes(destination, oldBytes);
            var result = installer.DownloadAndUpdateAsync(
                CreateModel(newBytes, newHash),
                destination).GetAwaiter().GetResult();

            Assert(File.Exists(destination), "A successful update must retain a model at the final path.");
            Assert(File.ReadAllBytes(destination).SequenceEqual(newBytes),
                "The final path must contain the newly downloaded bytes after validation.");
            Assert(!File.ReadAllBytes(destination).SequenceEqual(oldBytes),
                "A successful update must replace the older model bytes.");
            Assert(result.InstalledPath == destination, "An update should report the same final destination path.");
            Assert(result.SizeBytes == newBytes.LongLength, "The update result should report validated bytes.");
            Assert(result.Sha256 == newHash, "The update result should report the verified SHA-256 checksum.");
            Assert(Directory.GetFiles(folder).SequenceEqual(new[] { destination }),
                "A successful atomic update must not leave staging or backup files behind.");
            Assert(handler.RequestCount == 1, "A valid update should download exactly once.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void RejectsFailedUpdateAndPreservesPreviousModel()
    {
        var downloadedBytes = CreateModelBytes();
        var actualHash = Convert.ToHexString(SHA256.HashData(downloadedBytes)).ToLowerInvariant();
        var wrongHash = (actualHash[0] == '0' ? "1" : "0") + actualHash[1..];
        var oldBytes = new byte[] { 91, 92, 93, 94, 95 };
        var handler = CreateHandler(downloadedBytes);
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader);
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "model.gguf");

        try
        {
            File.WriteAllBytes(destination, oldBytes);
            ExpectInvalidData(
                () => installer.DownloadAndUpdateAsync(
                    CreateModel(downloadedBytes, wrongHash),
                    destination).GetAwaiter().GetResult(),
                "An update with an incorrect SHA-256 must fail.");
            Assert(File.ReadAllBytes(destination).SequenceEqual(oldBytes),
                "A checksum failure must preserve the previous model byte-for-byte.");
            Assert(handler.RequestCount == 1, "Checksum validation should occur after downloading the update.");
            Assert(Directory.GetFiles(folder).SequenceEqual(new[] { destination }),
                "A failed update must delete its staging file and keep the prior model without extra files.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void RejectsMissingUpdateTargetBeforeNetworkAccess()
    {
        var bytes = CreateModelBytes();
        var checksum = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var handler = CreateHandler(bytes);
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader);
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "missing-model.gguf");

        try
        {
            try
            {
                _ = installer.DownloadAndUpdateAsync(
                    CreateModel(bytes, checksum),
                    destination).GetAwaiter().GetResult();
                throw new InvalidOperationException("Updating a missing model must be refused.");
            }
            catch (FileNotFoundException)
            {
                // Expected: update is not the same operation as initial installation.
            }

            Assert(handler.RequestCount == 0, "A missing update target must be rejected before network access.");
            Assert(Directory.GetFiles(folder).Length == 0, "A missing update target must not create staging files.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void RefusesConcurrentDestinationChangeDuringUpdate()
    {
        var downloadedBytes = CreateModelBytes();
        var checksum = Convert.ToHexString(SHA256.HashData(downloadedBytes)).ToLowerInvariant();
        var originalBytes = new byte[] { 11, 12, 13 };
        var concurrentBytes = new byte[] { 21, 22, 23, 24, 25 };
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "model.gguf");
        TestHttpMessageHandler? handler = null;
        handler = new TestHttpMessageHandler(() =>
        {
            File.WriteAllBytes(destination, concurrentBytes);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(downloadedBytes)
            };
            response.Content.Headers.ContentLength = downloadedBytes.LongLength;
            return response;
        });
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader);

        try
        {
            File.WriteAllBytes(destination, originalBytes);
            try
            {
                _ = installer.DownloadAndUpdateAsync(
                    CreateModel(downloadedBytes, checksum),
                    destination).GetAwaiter().GetResult();
                throw new InvalidOperationException("An update must not overwrite a destination changed during download.");
            }
            catch (IOException exception) when (exception.Message.Contains("changed during update", StringComparison.Ordinal))
            {
                // Expected: the competing writer's version is preserved.
            }

            Assert(File.ReadAllBytes(destination).SequenceEqual(concurrentBytes),
                "A destination changed during the download must be preserved instead of overwritten.");
            Assert(handler.RequestCount == 1, "The competing change should be detected after one download.");
            Assert(Directory.GetFiles(folder).SequenceEqual(new[] { destination }),
                "Refusing a competing update must clean the staged replacement.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static byte[] CreateModelBytes()
    {
        var bytes = new byte[180 * 1024 + 19];
        for (var index = 8; index < bytes.Length; index++)
        {
            bytes[index] = (byte)((index * 17) % 251);
        }

        bytes[0] = (byte)'G';
        bytes[1] = (byte)'G';
        bytes[2] = (byte)'U';
        bytes[3] = (byte)'F';
        bytes[4] = 3; // GGUF v3, little-endian.
        bytes[5] = 0;
        bytes[6] = 0;
        bytes[7] = 0;
        return bytes;
    }

    private static ModelCatalogEntry CreateModel(
        byte[] bytes,
        string checksum,
        string checksumAlgorithm = "sha256",
        string format = "GGUF") =>
        new(
            "verified-install-test",
            "Verified install test model",
            format,
            bytes.LongLength,
            checksumAlgorithm,
            checksum,
            new[] { "llama.cpp" },
            new Uri("https://models.example.test/model.gguf"),
            new ModelLicenseMetadata("review_required", null, null));

    private static TestHttpMessageHandler CreateHandler(byte[] bytes) => new(() =>
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bytes)
        };
        response.Content.Headers.ContentLength = bytes.LongLength;
        return response;
    });

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "whitep-verified-install-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void ExpectInvalidData(Action action, string message)
    {
        try
        {
            action();
        }
        catch (InvalidDataException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class TestHttpMessageHandler(Func<HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            return Task.FromResult(responseFactory());
        }
    }
}
