namespace WhiteP.Host.Application;

public interface IInferenceBackend
{
    bool IsAvailable { get; }

    string UnavailableReason { get; }

    Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(IsAvailable);

    Task<string> GenerateAsync(string prompt, uint maxOutputTokens, CancellationToken cancellationToken = default) =>
        Task.FromException<string>(new NotSupportedException("This backend does not support text generation."));
}
