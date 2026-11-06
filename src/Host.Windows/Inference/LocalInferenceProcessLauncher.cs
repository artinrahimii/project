using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace WhiteP.Host.Windows.Inference;

/// <summary>Starts and owns a local llama.cpp server process without invoking a command shell.</summary>
public sealed class LocalInferenceProcessLauncher : IDisposable
{
    private static readonly TimeSpan DefaultStopTimeout = TimeSpan.FromSeconds(5);
    private readonly object _sync = new();
    private Process? _process;
    private bool _disposed;

    public Process Start(string executablePath, string modelPath, Uri serverAddress, string modelId)
    {
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_process is not null && !_process.HasExited)
                throw new InvalidOperationException("A local inference process started by this host is already running.");
            _process?.Dispose();
            _process = null;

            var expectedVersion = Environment.GetEnvironmentVariable("WHITEP_LLAMA_SERVER_VERSION");
            var expectedSha256 = Environment.GetEnvironmentVariable("WHITEP_LLAMA_SERVER_SHA256");
            if (string.IsNullOrWhiteSpace(expectedVersion) || string.IsNullOrWhiteSpace(expectedSha256))
                throw new InvalidOperationException("Set WHITEP_LLAMA_SERVER_VERSION and WHITEP_LLAMA_SERVER_SHA256 from a trusted llama.cpp release before starting the local engine.");
            var verification = LocalInferenceBinaryVerifier.Verify(executablePath, expectedVersion, expectedSha256);
            var process = new Process { StartInfo = CreateStartInfo(verification.ExecutablePath, modelPath, serverAddress, modelId), EnableRaisingEvents = true };
            try
            {
                if (!process.Start()) throw new InvalidOperationException("The local llama.cpp server process did not start.");
                _process = process;
                return process;
            }
            catch
            {
                process.Dispose();
                throw;
            }
        }
    }

    /// <summary>Creates a shell-free process description testable without installing a model.</summary>
    public static ProcessStartInfo CreateStartInfo(string executablePath, string modelPath, Uri serverAddress, string modelId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelPath);
        ArgumentNullException.ThrowIfNull(serverAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);

        var executableFullPath = Path.GetFullPath(executablePath.Trim());
        var modelFullPath = Path.GetFullPath(modelPath.Trim());
        if (!File.Exists(executableFullPath) ||
            !string.Equals(Path.GetFileName(executableFullPath), "llama-server.exe", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Select an existing llama-server.exe executable.", nameof(executablePath));
        if (!File.Exists(modelFullPath) ||
            !string.Equals(Path.GetExtension(modelFullPath), ".gguf", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Select an existing GGUF model file.", nameof(modelPath));

        if (!serverAddress.IsAbsoluteUri || serverAddress.Scheme != Uri.UriSchemeHttp ||
            !serverAddress.IsLoopback || serverAddress.UserInfo.Length != 0 ||
            serverAddress.Query.Length != 0 || serverAddress.Fragment.Length != 0 ||
            serverAddress.AbsolutePath != "/" || serverAddress.Port is < 1024 or > 65535)
            throw new ArgumentException("Managed server startup requires plain HTTP loopback on port 1024–65535.", nameof(serverAddress));

        var alias = modelId.Trim();
        if (alias.Length is < 1 or > 128 || !char.IsAsciiLetterOrDigit(alias[0]) ||
            alias.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '.' and not '_' and not '-'))
            throw new ArgumentException("Model ID must be 1–128 ASCII letters, digits, dots, underscores or hyphens and start with a letter or digit.", nameof(modelId));

        var info = new ProcessStartInfo
        {
            FileName = executableFullPath,
            WorkingDirectory = Path.GetDirectoryName(executableFullPath)!,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };
        // Keep every option/value in its own argv element; never concatenate input into Arguments.
        info.ArgumentList.Add("--model");
        info.ArgumentList.Add(modelFullPath);
        info.ArgumentList.Add("--host");
        info.ArgumentList.Add("127.0.0.1");
        info.ArgumentList.Add("--port");
        info.ArgumentList.Add(serverAddress.Port.ToString(CultureInfo.InvariantCulture));
        info.ArgumentList.Add("--alias");
        info.ArgumentList.Add(alias);
        info.ArgumentList.Add("--no-webui");
        return info;
    }

    /// <summary>Whether this launcher currently owns a live inference process.</summary>
    public bool IsRunning
    {
        get
        {
            lock (_sync)
            {
                return !_disposed && _process is not null && !_process.HasExited;
            }
        }
    }

    /// <summary>
    /// Stops the owned server process and its descendants. This is the cancellation fallback
    /// when the inference server continues computing after the client disconnects.
    /// The launcher remains reusable after Stop.
    /// </summary>
    public void Stop()
    {
        lock (_sync)
        {
            if (_disposed) return;
            var process = _process;
            _process = null;
            StopProcess(process);
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed) return;
            _disposed = true;
            var process = _process;
            _process = null;
            StopProcess(process);
        }
    }

    private static void StopProcess(Process? process)
    {
        if (process is null) return;
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                if (!process.WaitForExit((int)Math.Ceiling(DefaultStopTimeout.TotalMilliseconds)))
                    throw new TimeoutException("The local llama.cpp server did not stop within the allowed time.");
            }
        }
        catch (InvalidOperationException) when (process.HasExited)
        {
            // The child can exit while shutdown is being requested.
        }
        finally
        {
            process.Dispose();
        }
    }
}
