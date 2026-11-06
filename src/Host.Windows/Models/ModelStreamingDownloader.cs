using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using WhiteP.Host.Windows.Models.Catalog;

namespace WhiteP.Host.Windows.Models;

/// <summary>Progress for a streaming model download.</summary>
public sealed record ModelDownloadProgress(long BytesDownloaded, long TotalBytes)
{
    public TimeSpan Elapsed { get; init; }

    public double BytesPerSecond { get; init; }

    public double ProgressPercentage =>
        TotalBytes > 0 ? Math.Clamp(BytesDownloaded * 100d / TotalBytes, 0d, 100d) : 0d;
}

/// <summary>
/// Streams model bytes to a caller-selected path. The caller owns temporary-path policy and
/// checksum verification; this class never buffers the complete response in memory.
/// </summary>
public sealed class ModelStreamingDownloader : IDisposable
{
    public const int BufferSizeBytes = 64 * 1024;
    private const int MaximumRedirects = 5;

    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly HashSet<string> _allowedSourceHosts;

    public ModelStreamingDownloader(IEnumerable<string> allowedSourceHosts, HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(allowedSourceHosts);
        _allowedSourceHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var host in allowedSourceHosts)
        {
            _allowedSourceHosts.Add(NormalizeAllowedHost(host));
        }

        if (_allowedSourceHosts.Count == 0)
        {
            throw new ArgumentException("At least one allowed model source host is required.", nameof(allowedSourceHosts));
        }

        _httpClient = httpClient ?? CreateHttpClient();
        _ownsHttpClient = httpClient is null;
    }

    /// <summary>
    /// Downloads a catalog entry using a fixed-size buffer. The destination directory must already
    /// exist, and an existing destination file is never overwritten.
    /// </summary>
    public async Task<long> DownloadAsync(
        ModelCatalogEntry model,
        string destinationPath,
        IProgress<ModelDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        if (model.SizeBytes <= 0)
        {
            throw new InvalidDataException("Model metadata must declare a positive file size.");
        }

        ValidateSourceUrl(model.SourceUrl);
        var fullPath = Path.GetFullPath(destinationPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException("The model download destination directory must already exist.");
        }

        using var response = await SendForContentAsync(model.SourceUrl, cancellationToken).ConfigureAwait(false);
        if (response.Content.Headers.ContentLength is long contentLength && contentLength != model.SizeBytes)
        {
            throw new InvalidDataException("The model response Content-Length does not match validated catalog metadata.");
        }

        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var createdDestination = false;
        try
        {
            await using var output = new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                BufferSizeBytes,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            createdDestination = true;

            var buffer = new byte[BufferSizeBytes];
            var startedAt = Stopwatch.GetTimestamp();
            long totalBytes = 0;
            while (true)
            {
                var bytesRead = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)
                    .ConfigureAwait(false);
                if (bytesRead == 0)
                {
                    break;
                }

                totalBytes = checked(totalBytes + bytesRead);
                if (totalBytes > model.SizeBytes)
                {
                    throw new InvalidDataException("The model response exceeded its validated size.");
                }

                await output.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                var elapsed = Stopwatch.GetElapsedTime(startedAt);
                var bytesPerSecond = elapsed.TotalSeconds > 0 ? totalBytes / elapsed.TotalSeconds : 0d;
                progress?.Report(new ModelDownloadProgress(totalBytes, model.SizeBytes)
                {
                    Elapsed = elapsed,
                    BytesPerSecond = double.IsFinite(bytesPerSecond) ? bytesPerSecond : 0d
                });
            }

            await output.FlushAsync(cancellationToken).ConfigureAwait(false);
            if (totalBytes != model.SizeBytes)
            {
                throw new InvalidDataException("The model response ended before its validated size was received.");
            }

            return totalBytes;
        }
        catch
        {
            if (createdDestination)
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (IOException)
                {
                    // Preserve the original transfer or validation exception.
                }
                catch (UnauthorizedAccessException)
                {
                    // Preserve the original transfer or validation exception.
                }
            }

            throw;
        }
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private async Task<HttpResponseMessage> SendForContentAsync(Uri initialUri, CancellationToken cancellationToken)
    {
        var currentUri = initialUri;
        for (var redirectCount = 0; redirectCount <= MaximumRedirects; redirectCount++)
        {
            ValidateSourceUrl(currentUri);
            using var request = new HttpRequestMessage(HttpMethod.Get, currentUri);
            var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);

            if (IsRedirect(response.StatusCode))
            {
                var location = response.Headers.Location;
                response.Dispose();
                if (location is null)
                {
                    throw new HttpRequestException("The model source returned a redirect without a location.");
                }

                if (redirectCount == MaximumRedirects)
                {
                    throw new HttpRequestException("The model source exceeded the allowed redirect count.");
                }

                currentUri = location.IsAbsoluteUri ? location : new Uri(currentUri, location);
                ValidateSourceUrl(currentUri);
                continue;
            }

            try
            {
                if (response.RequestMessage?.RequestUri is Uri effectiveUri)
                {
                    ValidateSourceUrl(effectiveUri);
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        "The model source returned an unsuccessful HTTP status.",
                        null,
                        response.StatusCode);
                }

                return response;
            }
            catch
            {
                response.Dispose();
                throw;
            }
        }

        throw new HttpRequestException("The model source exceeded the allowed redirect count.");
    }

    private void ValidateSourceUrl(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!uri.IsAbsoluteUri ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            uri.HostNameType != UriHostNameType.Dns ||
            uri.IsLoopback ||
            uri.UserInfo.Length != 0 ||
            !uri.IsDefaultPort ||
            uri.Fragment.Length != 0 ||
            !_allowedSourceHosts.Contains(uri.IdnHost.TrimEnd('.').ToLowerInvariant()))
        {
            throw new InvalidDataException("The model source URL must use HTTPS and an explicitly allowed DNS host.");
        }
    }

    private static string NormalizeAllowedHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host) ||
            !string.Equals(host, host.Trim(), StringComparison.Ordinal) ||
            host.Contains("://", StringComparison.Ordinal) ||
            host.Contains('/') ||
            host.Contains('@') ||
            host.Contains('?') ||
            host.Contains('#') ||
            host.Contains(':') ||
            !Uri.TryCreate("https://" + host, UriKind.Absolute, out var uri) ||
            uri.HostNameType != UriHostNameType.Dns ||
            !uri.IsDefaultPort ||
            uri.AbsolutePath != "/" ||
            uri.Query.Length != 0 ||
            uri.Fragment.Length != 0 ||
            uri.UserInfo.Length != 0)
        {
            throw new ArgumentException("Allowed source entries must be bare DNS host names.", nameof(host));
        }

        var normalized = uri.IdnHost.TrimEnd('.').ToLowerInvariant();
        if (normalized.Length == 0 ||
            normalized == "localhost" ||
            normalized.EndsWith(".localhost", StringComparison.Ordinal) ||
            normalized.EndsWith(".local", StringComparison.Ordinal))
        {
            throw new ArgumentException("Local host names cannot be model source allowlist entries.", nameof(host));
        }

        return normalized;
    }

    private static HttpClient CreateHttpClient()
    {
        return new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
    }

    private static bool IsRedirect(HttpStatusCode statusCode) =>
        (int)statusCode is 301 or 302 or 303 or 307 or 308;
}
