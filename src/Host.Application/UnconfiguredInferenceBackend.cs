namespace WhiteP.Host.Application;

public sealed class UnconfiguredInferenceBackend : IInferenceBackend
{
    public bool IsAvailable => false;

    public string UnavailableReason => "Local inference backend is not configured. Enter a server URL and connect.";

    public Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default) => Task.FromResult(false);

    public Task<string> GenerateAsync(string prompt, uint maxOutputTokens, CancellationToken cancellationToken = default) =>
        Task.FromException<string>(new InvalidOperationException(UnavailableReason));
}
