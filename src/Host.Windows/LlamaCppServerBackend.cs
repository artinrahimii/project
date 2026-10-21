using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhiteP.Host.Windows;

/// <summary>A real HTTP client for a llama.cpp server running on the same host.</summary>
/// <remarks>The server/model must be installed and started separately. This class never logs prompt or response content.</remarks>
public sealed class LlamaCppServerBackend : IInferenceBackend, IDisposable
{
    private readonly HttpClient _http;
    private readonly bool _ownsClient;
    private readonly SemaphoreSlim _inferenceGate = new(1, 1);
    private readonly string _model;
    private readonly TimeSpan _requestTimeout;
    private volatile string _unavailableReason = "Local inference backend has not passed its health check.";

    public LlamaCppServerBackend(Uri serverBaseAddress, string model = "local-model", HttpClient? httpClient = null, TimeSpan? requestTimeout = null)
    {
        if (serverBaseAddress is null || !serverBaseAddress.IsAbsoluteUri ||
            (serverBaseAddress.Scheme != Uri.UriSchemeHttp && serverBaseAddress.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("An absolute HTTP(S) server URL is required.", nameof(serverBaseAddress));
        if (string.IsNullOrWhiteSpace(model)) throw new ArgumentException("A model identifier is required.", nameof(model));
        _model = model.Trim();
        _requestTimeout = requestTimeout ?? TimeSpan.FromMinutes(5);
        if (_requestTimeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(requestTimeout));
        _http = httpClient ?? new HttpClient();
        _ownsClient = httpClient is null;
        _http.BaseAddress = EnsureTrailingSlash(serverBaseAddress);
        _http.Timeout = Timeout.InfiniteTimeSpan;
    }

    public bool IsAvailable { get; private set; }
    public string UnavailableReason => _unavailableReason;

    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        using var timeout = CreateTimeout(cancellationToken);
        try
        {
            using var response = await _http.GetAsync("health", HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
            IsAvailable = response.IsSuccessStatusCode;
            _unavailableReason = IsAvailable ? string.Empty : $"Local inference backend health check returned HTTP {(int)response.StatusCode}.";
            return IsAvailable;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            IsAvailable = false;
            _unavailableReason = "Local inference backend health check timed out.";
            return false;
        }
        catch (HttpRequestException)
        {
            IsAvailable = false;
            _unavailableReason = "Local inference backend could not be reached.";
            return false;
        }
    }

    public async Task<string> GenerateAsync(string prompt, uint maxOutputTokens, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentException("Prompt must not be empty.", nameof(prompt));
        if (maxOutputTokens == 0 || maxOutputTokens > 131072) throw new ArgumentOutOfRangeException(nameof(maxOutputTokens), "Token limit must be between 1 and 131072.");
        if (!IsAvailable && !await CheckHealthAsync(cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException(UnavailableReason);

        await _inferenceGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            using var timeout = CreateTimeout(cancellationToken);
            using var response = await _http.PostAsJsonAsync("v1/chat/completions",
                new ChatCompletionRequest(_model, [new ChatMessage("user", prompt)], checked((int)maxOutputTokens)),
                JsonOptions, timeout.Token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                IsAvailable = response.StatusCode != HttpStatusCode.ServiceUnavailable &&
                              response.StatusCode != HttpStatusCode.BadGateway &&
                              response.StatusCode != HttpStatusCode.GatewayTimeout;
                _unavailableReason = $"Local inference backend returned HTTP {(int)response.StatusCode}.";
                throw new HttpRequestException(_unavailableReason, null, response.StatusCode);
            }

            var payload = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(JsonOptions, timeout.Token).ConfigureAwait(false);
            var text = payload?.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidDataException("Local inference backend returned no generated text.");
            IsAvailable = true;
            _unavailableReason = string.Empty;
            return text;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("Local inference request exceeded its configured deadline.");
        }
        catch (HttpRequestException)
        {
            IsAvailable = false;
            if (string.IsNullOrWhiteSpace(_unavailableReason))
                _unavailableReason = "Local inference backend request failed.";
            throw;
        }
        finally
        {
            _inferenceGate.Release();
        }
    }

    private CancellationTokenSource CreateTimeout(CancellationToken external)
    {
        var source = CancellationTokenSource.CreateLinkedTokenSource(external);
        source.CancelAfter(_requestTimeout);
        return source;
    }

    private static Uri EnsureTrailingSlash(Uri uri) =>
        uri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal) ? uri : new Uri(uri.AbsoluteUri + "/", UriKind.Absolute);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public void Dispose()
    {
        _inferenceGate.Dispose();
        if (_ownsClient) _http.Dispose();
    }

    private sealed record ChatCompletionRequest(string Model, IReadOnlyList<ChatMessage> Messages, int MaxTokens);
    private sealed record ChatMessage(string Role, string Content);
    private sealed record ChatCompletionResponse(IReadOnlyList<Choice>? Choices);
    private sealed record Choice(ChatMessage? Message);
}
