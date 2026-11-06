using System.IO;
using System.Net;
using System.Net.Http;
using WhiteP.Host.Windows.Models;
using WhiteP.Host.Windows.Models.Catalog;

internal static class ModelDownloadCancellationAcceptanceTests
{
    public static void Run()
    {
        CancellationAfterRealProgressRemovesStagingAndNeverPublishesFinalModel();
        Console.WriteLine("PASS: cancellation after real download progress leaves no partial staging file or final model.");
    }

    private static void CancellationAfterRealProgressRemovesStagingAndNeverPublishesFinalModel()
    {
        var bytes = new byte[2 * 1024 * 1024];
        for (var index = 0; index < bytes.Length; index++)
        {
            bytes[index] = (byte)(index % 239);
        }

        var handler = new TestHttpMessageHandler(() =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };
            response.Content.Headers.ContentLength = bytes.LongLength;
            return response;
        });
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader);
        using var cancellation = new CancellationTokenSource();
        var progress = new CancelAfterFirstChunkProgress(cancellation);
        var folder = Path.Combine(Path.GetTempPath(), "whitep-model-cancel-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var destination = Path.Combine(folder, "model.gguf");
        var model = new ModelCatalogEntry(
            "cancel-download-test",
            "Cancellation test model",
            "GGUF",
            bytes.LongLength,
            "sha256",
            new string('a', 64),
            new[] { "llama.cpp" },
            new Uri("https://models.example.test/model.gguf"),
            new ModelLicenseMetadata("review_required", null, null));

        try
        {
            try
            {
                _ = installer.DownloadAndInstallAsync(model, destination, progress, cancellation.Token)
                    .GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                Assert(handler.RequestCount == 1, "The cancellation test must start a real download request.");
                Assert(progress.BytesObserved > 0 && progress.BytesObserved < bytes.LongLength,
                    "Cancellation must occur after partial bytes have actually been reported.");
                Assert(!File.Exists(destination), "Cancellation must never publish an incomplete final model.");
                Assert(Directory.GetFiles(folder).Length == 0,
                    "Cancellation must remove the partial staging file.");
                return;
            }

            throw new InvalidOperationException("The transfer should stop after cancellation is requested.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
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
