using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using WhiteP.Host.Windows.Models;
using WhiteP.Host.Windows.Models.Catalog;

internal static class UpdateModelSafetyAcceptanceTests
{
    public static void Run()
    {
        RejectsInsufficientDiskSpaceBeforeNetworkAccess();
        HttpFailurePreservesExistingModel();
        RejectsMalformedSha256BeforeNetworkAccess();
        CancellationDuringUpdatePreservesExistingModel();
        Console.WriteLine("PASS: insufficient disk space, HTTP failure, invalid checksum, and cancelled updates preserve the existing model.");
    }

    private static void RejectsInsufficientDiskSpaceBeforeNetworkAccess()
    {
        var bytes = CreateModelBytes();
        var model = CreateModel(bytes, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
        var handler = CreateHandler(() =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
            response.Content.Headers.ContentLength = bytes.LongLength;
            return response;
        });
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader, _ => model.SizeBytes - 1);
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "model.gguf");
        var previousBytes = new byte[] { 71, 72, 73, 74 };

        try
        {
            File.WriteAllBytes(destination, previousBytes);
            try
            {
                _ = installer.DownloadAndUpdateAsync(model, destination).GetAwaiter().GetResult();
                throw new InvalidOperationException("The update must be refused when free space is less than the staging model size.");
            }
            catch (IOException exception) when (exception.Message.Contains("Insufficient free disk space", StringComparison.Ordinal))
            {
                // Expected: the disk-space preflight runs before the network request.
            }

            Assert(handler.RequestCount == 0, "Insufficient disk space must be detected before networking.");
            Assert(File.ReadAllBytes(destination).SequenceEqual(previousBytes),
                "Insufficient disk space must preserve the prior installed model byte-for-byte.");
            Assert(Directory.GetFiles(folder).SequenceEqual(new[] { destination }),
                "Insufficient disk space must not create staging or backup artifacts.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void HttpFailurePreservesExistingModel()
    {
        var bytes = CreateModelBytes();
        var model = CreateModel(bytes, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
        var handler = CreateHandler(() => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        {
            Content = new ByteArrayContent(Array.Empty<byte>())
        });
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader);
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "model.gguf");
        var previousBytes = new byte[] { 81, 82, 83, 84, 85 };

        try
        {
            File.WriteAllBytes(destination, previousBytes);
            try
            {
                _ = installer.DownloadAndUpdateAsync(model, destination).GetAwaiter().GetResult();
                throw new InvalidOperationException("An unsuccessful HTTP response must not be installed.");
            }
            catch (HttpRequestException)
            {
                Assert(handler.RequestCount == 1, "An HTTP failure must be observed after one network request.");
                Assert(File.ReadAllBytes(destination).SequenceEqual(previousBytes),
                    "An HTTP failure must leave the currently installed model unchanged.");
                Assert(Directory.GetFiles(folder).SequenceEqual(new[] { destination }),
                    "An HTTP failure must not leave staging or backup artifacts.");
                return;
            }
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void RejectsMalformedSha256BeforeNetworkAccess()
    {
        var bytes = CreateModelBytes();
        var handler = CreateHandler(() =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
            response.Content.Headers.ContentLength = bytes.LongLength;
            return response;
        });
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader);
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "model.gguf");
        var previousBytes = new byte[] { 31, 32, 33, 34, 35 };

        try
        {
            File.WriteAllBytes(destination, previousBytes);
            try
            {
                _ = installer.DownloadAndUpdateAsync(CreateModel(bytes, "not-a-sha256-checksum"), destination)
                    .GetAwaiter().GetResult();
                throw new InvalidOperationException("Malformed checksum metadata must be rejected.");
            }
            catch (InvalidDataException)
            {
                Assert(handler.RequestCount == 0, "Invalid checksum metadata must fail before networking.");
                Assert(File.ReadAllBytes(destination).SequenceEqual(previousBytes),
                    "Invalid checksum metadata must preserve the previous file.");
                Assert(Directory.GetFiles(folder).SequenceEqual(new[] { destination }),
                    "Invalid checksum metadata must not create temporary artifacts.");
            }
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void CancellationDuringUpdatePreservesExistingModel()
    {
        var bytes = CreateModelBytes(2 * 1024 * 1024);
        var model = CreateModel(bytes, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
        var handler = CreateHandler(() =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
            response.Content.Headers.ContentLength = bytes.LongLength;
            return response;
        });
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader);
        using var cancellation = new CancellationTokenSource();
        var progress = new CancelAfterFirstChunkProgress(cancellation);
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "model.gguf");
        var previousBytes = new byte[] { 41, 42, 43, 44, 45 };

        try
        {
            File.WriteAllBytes(destination, previousBytes);
            try
            {
                _ = installer.DownloadAndUpdateAsync(model, destination, progress, cancellation.Token)
                    .GetAwaiter().GetResult();
                throw new InvalidOperationException("A cancelled update must not replace the installed model.");
            }
            catch (OperationCanceledException)
            {
                Assert(handler.RequestCount == 1, "Cancellation must occur during an actual download request.");
                Assert(progress.BytesObserved > 0 && progress.BytesObserved < model.SizeBytes,
                    "Cancellation must be triggered after partial bytes are received.");
                Assert(File.ReadAllBytes(destination).SequenceEqual(previousBytes),
                    "A cancelled update must preserve the previous file byte-for-byte.");
                Assert(Directory.GetFiles(folder).SequenceEqual(new[] { destination }),
                    "Cancellation must remove the staging file and leave no backup artifact.");
            }
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static byte[] CreateModelBytes(int size = 512 * 1024)
    {
        var bytes = new byte[size];
        for (var index = 8; index < bytes.Length; index++)
        {
            bytes[index] = (byte)((index * 17) % 251);
        }

        bytes[0] = (byte)'G';
        bytes[1] = (byte)'G';
        bytes[2] = (byte)'U';
        bytes[3] = (byte)'F';
        bytes[4] = 3;
        bytes[5] = 0;
        bytes[6] = 0;
        bytes[7] = 0;
        return bytes;
    }

    private static ModelCatalogEntry CreateModel(byte[] bytes, string checksum) =>
        new(
            "update-safety-test",
            "Update safety test model",
            "GGUF",
            bytes.LongLength,
            "sha256",
            checksum,
            new[] { "llama.cpp" },
            new Uri("https://models.example.test/model.gguf"),
            new ModelLicenseMetadata("review_required", null, null));

    private static TestHttpMessageHandler CreateHandler(Func<HttpResponseMessage> responseFactory) =>
        new(responseFactory);

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "whitep-model-update-safety-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class CancelAfterFirstChunkProgress(CancellationTokenSource cancellation) : IProgress<ModelDownloadProgress>
    {
        public long BytesObserved { get; private set; }

        public void Report(ModelDownloadProgress value)
        {
            if (BytesObserved == 0 && value.BytesDownloaded > 0 && value.BytesDownloaded < value.TotalBytes)
            {
                BytesObserved = value.BytesDownloaded;
                cancellation.Cancel();
            }
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
