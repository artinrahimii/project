using System.IO;
using System.Text.Json;
using WhiteP.Host.Windows.Chat;

namespace WhiteP.Host.Windows.Tools;

/// <summary>
/// Worker-side half of a strict, single-request JSON protocol. The worker resolves the requested
/// name against its own explicit allowlist and validates arguments again before calling tool code.
/// It deliberately emits no logs or exception details to standard output.
/// </summary>
public static class AgentToolWorkerHost
{
    public const int MaximumRequestBytes = 64 * 1024;
    public const int MaximumResponseBytes = 64 * 1024;

    public static async Task<int> RunAsync(
        Stream input,
        Stream output,
        AgentToolRegistry registry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(registry);

        try
        {
            var requestBytes = await ReadBoundedAsync(input, MaximumRequestBytes, cancellationToken)
                .ConfigureAwait(false);
            using var request = JsonDocument.Parse(
                requestBytes,
                new JsonDocumentOptions { MaxDepth = 16 });

            var root = request.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                root.EnumerateObject().Count() != 2 ||
                !root.TryGetProperty("name", out var nameElement) ||
                nameElement.ValueKind != JsonValueKind.String ||
                !root.TryGetProperty("arguments", out var argumentsElement) ||
                argumentsElement.ValueKind != JsonValueKind.Object)
            {
                await WriteFailureAsync(output, cancellationToken).ConfigureAwait(false);
                return 2;
            }

            var call = new AgentToolCall(nameElement.GetString()!, argumentsElement);
            var tool = registry.ResolveValidated(call);
            var result = await tool.ExecuteAsync(call.Arguments.Clone(), cancellationToken)
                .ConfigureAwait(false);

            if (result is null || result.Content is null)
            {
                await WriteFailureAsync(output, cancellationToken).ConfigureAwait(false);
                return 2;
            }

            var responseBytes = JsonSerializer.SerializeToUtf8Bytes(new
            {
                ok = true,
                content = result.Content,
                isError = result.IsError
            });

            if (responseBytes.Length > MaximumResponseBytes)
            {
                await WriteFailureAsync(output, cancellationToken).ConfigureAwait(false);
                return 2;
            }

            await output.WriteAsync(responseBytes.AsMemory(), cancellationToken).ConfigureAwait(false);
            await output.FlushAsync(cancellationToken).ConfigureAwait(false);
            return 0;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return 3;
        }
        catch
        {
            await WriteFailureAsync(output, cancellationToken).ConfigureAwait(false);
            return 2;
        }
    }

    private static async Task<byte[]> ReadBoundedAsync(
        Stream input,
        int maximumBytes,
        CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[4096];
        while (true)
        {
            var count = await input.ReadAsync(chunk.AsMemory(), cancellationToken).ConfigureAwait(false);
            if (count == 0)
            {
                break;
            }

            if (buffer.Length + count > maximumBytes)
            {
                throw new InvalidDataException("The worker request exceeded its size limit.");
            }

            await buffer.WriteAsync(chunk.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
        }

        return buffer.ToArray();
    }

    private static async Task WriteFailureAsync(Stream output, CancellationToken cancellationToken)
    {
        var responseBytes = JsonSerializer.SerializeToUtf8Bytes(new
        {
            ok = false,
            errorCode = "worker_rejected"
        });
        await output.WriteAsync(responseBytes.AsMemory(), cancellationToken).ConfigureAwait(false);
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
