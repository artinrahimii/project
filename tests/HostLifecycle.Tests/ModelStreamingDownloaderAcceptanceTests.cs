using System.IO;
using System.Net;
using System.Net.Http;
using WhiteP.Host.Windows.Models;
using WhiteP.Host.Windows.Models.Catalog;

internal static class ModelStreamingDownloaderAcceptanceTests
{
    public static void Run()
    {
        StreamsModelInBoundedChunksAndReportsProgress();
        RejectsUnapprovedSourceAndRedirectHosts();
        RejectsTruncatedPayloadAndRemovesPartialFile();
        Console.WriteLine("PASS: model downloads stream bounded chunks, enforce HTTPS source allowlists and declared size, and remove partial outputs.");
    }

    private static void StreamsModelInBoundedChunksAndReportsProgress()
    {
        var bytes = new byte[256 * 1024 + 123];
        for (var index = 0; index < bytes.Length; index++)
        {
            bytes[index] = (byte)(index % 251);
        }

        var stream = new LimitedReadStream(bytes, 4096);
        var handler = new TestHttpMessageHandler(() =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };
            response.Content.Headers.ContentLength = bytes.LongLength;
            return response;
        });
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "model.gguf");
        var progress = new RecordingProgress();

        try
        {
            var received = downloader.DownloadAsync(
                CreateModel(bytes.LongLength, new Uri("https://models.example.test/model.gguf")),
                destination,
                progress).GetAwaiter().GetResult();

            Assert(received == bytes.LongLength, "The downloader must report the exact bytes written.");
            Assert(handler.RequestCount == 1, "A valid source should issue exactly one GET request.");
            Assert(stream.ReadCalls > 10, "The response should be read in multiple bounded chunks.");
            Assert(stream.MaxRequestedBufferLength <= ModelStreamingDownloader.BufferSizeBytes,
                "The downloader must not request an unbounded response buffer.");
            Assert(progress.Updates.Count > 1, "Progress should be reported as chunks arrive.");
            var finalProgress = progress.Updates[^1];
            Assert(finalProgress.BytesDownloaded == bytes.LongLength,
                "The final progress report should match the completed file size.");
            Assert(finalProgress.ProgressPercentage == 100d,
                "A completed stream must report exactly 100 percent progress.");
            Assert(double.IsFinite(finalProgress.BytesPerSecond) && finalProgress.BytesPerSecond >= 0d,
                "The estimated throughput must be a finite non-negative value.");
            Assert(finalProgress.Elapsed >= TimeSpan.Zero,
                "Elapsed transfer time cannot be negative.");
            long priorBytes = 0;
            double priorPercentage = 0d;
            TimeSpan priorElapsed = TimeSpan.Zero;
            foreach (var update in progress.Updates)
            {
                Assert(update.BytesDownloaded >= priorBytes && update.BytesDownloaded <= update.TotalBytes,
                    "Progress byte counts must be monotonic and never exceed the declared total.");
                Assert(update.ProgressPercentage >= priorPercentage,
                    "Progress percentage must not move backward.");
                Assert(update.Elapsed >= priorElapsed,
                    "Elapsed transfer time must be monotonic.");
                if (update.Elapsed.TotalSeconds > 0d)
                {
                    var expectedRate = update.BytesDownloaded / update.Elapsed.TotalSeconds;
                    Assert(Math.Abs(update.BytesPerSecond - expectedRate) < 0.001d,
                        "Reported throughput must be derived from the actual bytes and elapsed time.");
                }

                priorBytes = update.BytesDownloaded;
                priorPercentage = update.ProgressPercentage;
                priorElapsed = update.Elapsed;
            }

            Assert(File.ReadAllBytes(destination).SequenceEqual(bytes),
                "The streamed file must preserve every downloaded byte in order.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void RejectsUnapprovedSourceAndRedirectHosts()
    {
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "model.gguf");
        try
        {
            var initialHandler = new TestHttpMessageHandler(() => new HttpResponseMessage(HttpStatusCode.OK));
            using (var client = new HttpClient(initialHandler, disposeHandler: false))
            using (var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client))
            {
                ExpectInvalidData(
                    () => downloader.DownloadAsync(
                        CreateModel(1, new Uri("https://evil.example.test/model.gguf")),
                        destination).GetAwaiter().GetResult(),
                    "An unallowlisted initial source must be rejected.");
                Assert(initialHandler.RequestCount == 0, "An unallowlisted source must not receive a request.");
            }

            var redirectHandler = new TestHttpMessageHandler(() =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.Redirect);
                response.Headers.Location = new Uri("https://evil.example.test/model.gguf");
                return response;
            });
            using (var client = new HttpClient(redirectHandler, disposeHandler: false))
            using (var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client))
            {
                ExpectInvalidData(
                    () => downloader.DownloadAsync(
                        CreateModel(1, new Uri("https://models.example.test/model.gguf")),
                        destination).GetAwaiter().GetResult(),
                    "A redirect outside the allowlist must be rejected.");
                Assert(redirectHandler.RequestCount == 1, "A denied cross-host redirect must not be followed.");
            }

            Assert(!File.Exists(destination), "Rejected sources must not create a destination file.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void RejectsTruncatedPayloadAndRemovesPartialFile()
    {
        var bytes = new byte[16 * 1024];
        var stream = new LimitedReadStream(bytes, 2048);
        var handler = new TestHttpMessageHandler(() => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(stream)
        });
        using var client = new HttpClient(handler, disposeHandler: false);
        using var downloader = new ModelStreamingDownloader(new[] { "models.example.test" }, client);
        var folder = CreateTemporaryDirectory();
        var destination = Path.Combine(folder, "partial.gguf");

        try
        {
            ExpectInvalidData(
                () => downloader.DownloadAsync(
                    CreateModel(bytes.LongLength + 1, new Uri("https://models.example.test/model.gguf")),
                    destination).GetAwaiter().GetResult(),
                "A payload shorter than catalog metadata must be rejected.");
            Assert(!File.Exists(destination), "A rejected partial download must be deleted.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static ModelCatalogEntry CreateModel(long sizeBytes, Uri sourceUrl) =>
        new(
            "stream-test",
            "Streaming test model",
            "GGUF",
            sizeBytes,
            "sha256",
            new string('a', 64),
            new[] { "llama.cpp" },
            sourceUrl,
            new ModelLicenseMetadata("review_required", null, null));

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "whitep-model-stream-" + Guid.NewGuid().ToString("N"));
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

    private sealed class RecordingProgress : IProgress<ModelDownloadProgress>
    {
        public List<ModelDownloadProgress> Updates { get; } = new();

        public void Report(ModelDownloadProgress value) => Updates.Add(value);
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

    private sealed class LimitedReadStream(byte[] bytes, int maxChunkSize) : Stream
    {
        private readonly MemoryStream _inner = new(bytes, writable: false);

        public int ReadCalls { get; private set; }

        public int MaxRequestedBufferLength { get; private set; }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _inner.Length;

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
            MaxRequestedBufferLength = Math.Max(MaxRequestedBufferLength, count);
            ReadCalls++;
            return _inner.Read(buffer, offset, Math.Min(count, maxChunkSize));
        }

        public override int Read(Span<byte> buffer)
        {
            MaxRequestedBufferLength = Math.Max(MaxRequestedBufferLength, buffer.Length);
            ReadCalls++;
            return _inner.Read(buffer[..Math.Min(buffer.Length, maxChunkSize)]);
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            MaxRequestedBufferLength = Math.Max(MaxRequestedBufferLength, buffer.Length);
            ReadCalls++;
            var bounded = buffer[..Math.Min(buffer.Length, maxChunkSize)];
            return _inner.ReadAsync(bounded, cancellationToken);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
