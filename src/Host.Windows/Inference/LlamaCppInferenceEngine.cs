using System.Net.Http;
using System.Text.Json;
using WhiteP.Host.Application.Inference;
using WhiteP.Host.Infrastructure;

namespace WhiteP.Host.Windows.Inference;

/// <summary>
/// Connects the Windows host to an already-running local llama.cpp server.
/// The server owns model loading; this adapter verifies the configured model through
/// the server's OpenAI-compatible models endpoint before enabling generation.
/// </summary>
public sealed class LlamaCppInferenceEngine : IInferenceEngine
{
    private readonly Uri _serverAddress;
    private readonly string _configuredModelId;
    private readonly HttpClient _http;
    private readonly SemaphoreSlim _operationGate = new(1, 1);
    private readonly TimeSpan _probeTimeout;
    private LlamaCppServerBackend? _backend;
    private CancellationTokenSource? _activeGeneration;
    private bool _modelLoaded;
    private bool _disposed;

    public LlamaCppInferenceEngine(
        Uri serverAddress,
        string modelId,
        HttpClient? httpClient = null,
        TimeSpan? probeTimeout = null)
    {
        if (serverAddress is null || !serverAddress.IsAbsoluteUri ||
            (serverAddress.Scheme != Uri.UriSchemeHttp && serverAddress.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("An absolute HTTP(S) local server URL is required.", nameof(serverAddress));
        }

        if (string.IsNullOrWhiteSpace(modelId))
        {
            throw new ArgumentException("A configured local model identifier is required.", nameof(modelId));
        }

        _serverAddress = EnsureTrailingSlash(serverAddress);
        _configuredModelId = modelId.Trim();
        _probeTimeout = probeTimeout ?? TimeSpan.FromSeconds(5);
        if (_probeTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(probeTimeout));
        }

        _http = httpClient ?? new HttpClient();
        _http.BaseAddress = _serverAddress;
        _http.Timeout = Timeout.InfiniteTimeSpan;
        _backend = new LlamaCppServerBackend(_serverAddress, _configuredModelId, _http);
    }

    public async Task<InferenceEngineDiscoveryResult> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        using var timeout = CreateProbeTimeout(cancellationToken);
        try
        {
            var backend = EnsureBackend();
            if (!await backend.CheckHealthAsync(timeout.Token).ConfigureAwait(false))
            {
                return Unavailable(backend.UnavailableReason);
            }

            using var response = await _http.GetAsync("v1/models", HttpCompletionOption.ResponseHeadersRead, timeout.Token)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return Unavailable($"Local model discovery returned HTTP {(int)response.StatusCode}.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token).ConfigureAwait(false);
            using var payload = await JsonDocument.ParseAsync(stream, cancellationToken: timeout.Token).ConfigureAwait(false);
            if (!payload.RootElement.TryGetProperty("data", out var models) || models.ValueKind != JsonValueKind.Array)
            {
                return Unavailable("Local model discovery returned an unsupported response.");
            }

            var configuredModelFound = models.EnumerateArray().Any(model =>
                model.ValueKind == JsonValueKind.Object &&
                model.TryGetProperty("id", out var id) &&
                id.ValueKind == JsonValueKind.String &&
                string.Equals(id.GetString(), _configuredModelId, StringComparison.Ordinal));

            return configuredModelFound
                ? new InferenceEngineDiscoveryResult("llama.cpp", "Local llama.cpp server", true, null)
                : Unavailable("The configured model ID was not reported by the local server.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Unavailable("Local backend discovery timed out.");
        }
        catch (HttpRequestException)
        {
            return Unavailable("Local backend could not be reached.");
        }
        catch (JsonException)
        {
            return Unavailable("Local model discovery returned invalid JSON.");
        }
    }

    public async Task LoadModelAsync(string modelId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(modelId) ||
            !string.Equals(modelId.Trim(), _configuredModelId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The requested model ID does not match the model configured for this local server.");
        }

        await _operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var discovery = await DiscoverAsync(cancellationToken).ConfigureAwait(false);
            if (!discovery.IsAvailable)
            {
                throw new InvalidOperationException(discovery.UnavailableReason ?? "Local model is unavailable.");
            }

            // llama.cpp server loads its model at process startup; this confirms the
            // configured model is actually advertised rather than pretending to load it here.
            _modelLoaded = true;
        }
        finally
        {
            _operationGate.Release();
        }
    }

    public async Task<string> GenerateAsync(
        string prompt,
        uint maxOutputTokens,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (!_modelLoaded)
        {
            throw new InvalidOperationException("Load and verify the configured local model before generating text.");
        }

        await _operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            using var active = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _activeGeneration = active;
            try
            {
                return await EnsureBackend().GenerateAsync(prompt, maxOutputTokens, active.Token).ConfigureAwait(false);
            }
            finally
            {
                if (ReferenceEquals(_activeGeneration, active))
                {
                    _activeGeneration = null;
                }
            }
        }
        finally
        {
            _operationGate.Release();
        }
    }

    public Task CancelAsync()
    {
        ThrowIfDisposed();
        try
        {
            _activeGeneration?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Generation completed concurrently with this cancellation request.
        }

        return Task.CompletedTask;
    }

    public async Task UnloadModelAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        await _operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _modelLoaded = false;
            // The remote server owns model memory and exposes no portable unload API.
            // Dispose only this client's backend resources; do not claim server memory was freed.
            _backend?.Dispose();
            _backend = null;
        }
        finally
        {
            _operationGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _activeGeneration?.Cancel();
        await _operationGate.WaitAsync().ConfigureAwait(false);
        try
        {
            _modelLoaded = false;
            _backend?.Dispose();
            _backend = null;
            _disposed = true;
        }
        finally
        {
            _operationGate.Release();
            _operationGate.Dispose();
            _http.Dispose();
        }
    }

    private LlamaCppServerBackend EnsureBackend()
    {
        ThrowIfDisposed();
        return _backend ??= new LlamaCppServerBackend(_serverAddress, _configuredModelId, _http);
    }

    private CancellationTokenSource CreateProbeTimeout(CancellationToken external)
    {
        var timeout = CancellationTokenSource.CreateLinkedTokenSource(external);
        timeout.CancelAfter(_probeTimeout);
        return timeout;
    }

    private InferenceEngineDiscoveryResult Unavailable(string reason) =>
        new("llama.cpp", "Local llama.cpp server", false, reason);

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private static Uri EnsureTrailingSlash(Uri uri) =>
        uri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal)
            ? uri
            : new Uri(uri.AbsoluteUri + "/", UriKind.Absolute);
}
