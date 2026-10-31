using System.IO;
using System.Net;
using System.Net.Http;
using WhiteP.Host.Windows.Models;
using WhiteP.Host.Windows.Models.Catalog;

internal static class IncompleteModelDownloadAcceptanceTests
{
    public static void Run()
    {
        InterruptedTransferRemovesPartialStagingAndLeavesNoFinalModel();
        Console.WriteLine("PASS: interrupted model downloads remove partial staging files and never expose an incomplete final model.");
    }

    private static void InterruptedTransferRemovesPartialStagingAndLeavesNoFinalModel()
    {
        const int expectedSize = 64 * 1024;
        var handler = new InterruptingHttpMessageHandler(expectedSize);
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var installer = new VerifiedModelInstaller(downloader);
        var folder = Path.Combine(Path.GetTempPath(), "whitep-incomplete-download-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var destination = Path.Combine(folder, "model.gguf");
        var model = new ModelCatalogEntry(
            "interrupted-download-test",
            "Interrupted download test model",
            "GGUF",
            expectedSize,
            "sha256",
            new string('a', 64),
            new[] { "llama.cpp" },
            new Uri("https://models.example.test/model.gguf"),
            new ModelLicenseMetadata("review_required", null, null));

        try
        {
            try
            {
                _ = installer.DownloadAndInstallAsync(model, destination).GetAwaiter().GetResult();
            }
            catch (IOException)
            {
                Assert(handler.RequestCount == 1, "The transfer must start before the simulated network failure.");
                Assert(handler.Stream.BytesReturned > 0, "The fixture must deliver some data before it fails.");
                Assert(!File.Exists(destination), "An interrupted transfer must never expose a partial final model.");
                Assert(Directory.GetFiles(folder).Length == 0,
                    "The incomplete staging file must be removed after a network read failure.");
                return;
            }

            throw new InvalidOperationException("The interrupted response should fail instead of installing incomplete data.");
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

    private sealed class InterruptingHttpMessageHandler(long declaredLength) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        public InterruptingReadStream Stream { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(Stream)
            };
            response.Content.Headers.ContentLength = declaredLength;
            return Task.FromResult(response);
        }
    }

    private sealed class InterruptingReadStream : Stream
    {
        private readonly byte[] _firstChunk = Enumerable.Range(0, 8192).Select(value => (byte)(value % 251)).ToArray();
        private bool _sentFirstChunk;

        public int BytesReturned { get; private set; }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_sentFirstChunk)
            {
                throw new IOException("Simulated network interruption after a partial response.");
            }

            var bytes = Math.Min(count, _firstChunk.Length);
            Array.Copy(_firstChunk, 0, buffer, offset, bytes);
            _sentFirstChunk = true;
            BytesReturned += bytes;
            return bytes;
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            if (_sentFirstChunk)
            {
                return ValueTask.FromException<int>(
                    new IOException("Simulated network interruption after a partial response."));
            }

            var bytes = Math.Min(buffer.Length, _firstChunk.Length);
            _firstChunk.AsMemory(0, bytes).CopyTo(buffer);
            _sentFirstChunk = true;
            BytesReturned += bytes;
            return ValueTask.FromResult(bytes);
        }

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            return ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
