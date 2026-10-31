using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using WhiteP.Host.Windows.Chat;

namespace WhiteP.Host.Windows.Tools;

/// <summary>
/// Runs allowlisted tools in a child worker without a shell, inherited application secrets,
/// unbounded input/output, or an unbounded lifetime. This is a process/protocol boundary, not
/// an OS-level AppContainer or restricted-token sandbox; the worker still runs as the same user.
/// </summary>
public sealed class AgentToolProcessExecutor : IAgentToolProcessExecutor
{
    private const int MaximumStandardErrorCharacters = 16 * 1024;

    private static readonly HashSet<string> ForbiddenWorkerNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "bash", "bash.exe", "cmd", "cmd.exe", "fish", "fish.exe",
        "powershell", "powershell.exe", "pwsh", "pwsh.exe",
        "sh", "sh.exe", "zsh", "zsh.exe"
    };

    private static readonly string[] InheritedEnvironmentAllowlist =
    [
        "SystemRoot",
        "WINDIR",
        "TEMP",
        "TMP",
        "DOTNET_ROOT",
        "DOTNET_ROOT_X64",
        "DOTNET_ROOT_X86",
        "DOTNET_MULTILEVEL_LOOKUP"
    ];

    private readonly string _workerExecutablePath;
    private readonly string[] _workerArguments;
    private readonly TimeSpan _timeout;

    public AgentToolProcessExecutor(
        string workerExecutablePath,
        IEnumerable<string>? workerArguments = null,
        TimeSpan? timeout = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerExecutablePath);
        if (!Path.IsPathFullyQualified(workerExecutablePath))
        {
            throw new ArgumentException("The trusted worker executable path must be absolute.", nameof(workerExecutablePath));
        }

        _workerExecutablePath = Path.GetFullPath(workerExecutablePath);
        if (ForbiddenWorkerNames.Contains(Path.GetFileName(_workerExecutablePath)))
        {
            throw new ArgumentException("A command shell cannot be used as the agent-tool worker.", nameof(workerExecutablePath));
        }

        if (!File.Exists(_workerExecutablePath))
        {
            throw new FileNotFoundException("The trusted agent-tool worker executable was not found.", _workerExecutablePath);
        }

        _workerArguments = workerArguments?.ToArray() ?? Array.Empty<string>();
        if (_workerArguments.Length > 16 ||
            _workerArguments.Any(argument =>
                argument is null ||
                argument.Length > 2048 ||
                argument.Any(char.IsControl) ||
                string.Equals(argument, "--agent-tool-worker", StringComparison.Ordinal)))
        {
            throw new ArgumentException("Worker arguments must be a small, trusted, code-defined argument list.", nameof(workerArguments));
        }

        _timeout = timeout ?? TimeSpan.FromSeconds(15);
        if (_timeout < TimeSpan.FromMilliseconds(100) || _timeout > TimeSpan.FromMinutes(1))
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Worker timeout must be between 100 ms and 60 seconds.");
        }
    }

    /// <summary>Uses this application executable as the worker; the worker-side registry is empty by default.</summary>
    public static AgentToolProcessExecutor ForCurrentApplication() =>
        new(
            Environment.ProcessPath
                ?? throw new InvalidOperationException("The current application executable path is unavailable."),
            timeout: TimeSpan.FromSeconds(15));

    /// <summary>
    /// Creates a shell-free worker launch description. Only trusted constructor-supplied arguments
    /// enter argv; the model's tool name and JSON arguments travel over redirected standard input.
    /// </summary>
    public ProcessStartInfo CreateStartInfo()
    {
        var info = new ProcessStartInfo
        {
            FileName = _workerExecutablePath,
            WorkingDirectory = Path.GetDirectoryName(_workerExecutablePath)!,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            StandardOutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            StandardErrorEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        };

        foreach (var argument in _workerArguments)
        {
            info.ArgumentList.Add(argument);
        }

        info.ArgumentList.Add("--agent-tool-worker");

        // Do not pass API keys, provider credentials, or arbitrary caller environment variables to the worker.
        info.Environment.Clear();
        foreach (var name in InheritedEnvironmentAllowlist)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrEmpty(value))
            {
                info.Environment[name] = value;
            }
        }

        return info;
    }

    public async Task<AgentToolResult> ExecuteAsync(
        AgentToolCall call,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(call);
        cancellationToken.ThrowIfCancellationRequested();

        var requestJson = JsonSerializer.Serialize(new
        {
            name = call.Name,
            arguments = call.Arguments
        });
        if (Encoding.UTF8.GetByteCount(requestJson) > AgentToolWorkerHost.MaximumRequestBytes)
        {
            throw new InvalidDataException("The agent-tool request exceeded its size limit.");
        }

        using var process = new Process
        {
            StartInfo = CreateStartInfo(),
            EnableRaisingEvents = true
        };

        using var operationTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        operationTimeout.CancelAfter(_timeout);

        using var restrictedJob = WindowsWorkerJob.CreateIfSupported();
        if (!process.Start())
        {
            throw new AgentToolWorkerFailedException();
        }

        try
        {
            // On Windows, fail closed unless the worker is attached to a job that kills its
            // process tree on close and prevents the worker from creating additional processes.
            restrictedJob?.Assign(process);

            var stdoutTask = ReadBoundedAsync(
                process.StandardOutput,
                AgentToolWorkerHost.MaximumResponseBytes,
                operationTimeout.Token);
            var stderrTask = ReadBoundedAsync(
                process.StandardError,
                MaximumStandardErrorCharacters,
                operationTimeout.Token);

            await process.StandardInput.WriteAsync(requestJson.AsMemory(), operationTimeout.Token)
                .ConfigureAwait(false);
            await process.StandardInput.FlushAsync(operationTimeout.Token).ConfigureAwait(false);
            process.StandardInput.Close();

            var waitTask = process.WaitForExitAsync(operationTimeout.Token);
            var pending = new List<Task> { waitTask, stdoutTask, stderrTask };
            while (pending.Contains(waitTask))
            {
                var completed = await Task.WhenAny(pending).ConfigureAwait(false);
                if (completed == waitTask)
                {
                    await waitTask.ConfigureAwait(false);
                    pending.Remove(waitTask);
                    break;
                }

                await completed.ConfigureAwait(false);
                pending.Remove(completed);
            }

            var standardOutput = await stdoutTask.ConfigureAwait(false);
            _ = await stderrTask.ConfigureAwait(false);

            if (process.ExitCode != 0 ||
                Encoding.UTF8.GetByteCount(standardOutput) > AgentToolWorkerHost.MaximumResponseBytes)
            {
                throw new AgentToolWorkerFailedException();
            }

            using var response = JsonDocument.Parse(
                standardOutput,
                new JsonDocumentOptions { MaxDepth = 8 });
            var root = response.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                root.EnumerateObject().Count() != 3 ||
                !root.TryGetProperty("ok", out var ok) ||
                ok.ValueKind != JsonValueKind.True ||
                !root.TryGetProperty("content", out var content) ||
                content.ValueKind != JsonValueKind.String ||
                !root.TryGetProperty("isError", out var isError) ||
                (isError.ValueKind != JsonValueKind.True && isError.ValueKind != JsonValueKind.False))
            {
                throw new AgentToolWorkerFailedException();
            }

            return new AgentToolResult(content.GetString()!, isError.GetBoolean());
        }
        catch (OperationCanceledException) when (operationTimeout.IsCancellationRequested)
        {
            await KillAndWaitAsync(process).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            throw new TimeoutException("The agent-tool worker exceeded its execution timeout.");
        }
        catch
        {
            await KillAndWaitAsync(process).ConfigureAwait(false);
            throw;
        }
    }

    private static async Task<string> ReadBoundedAsync(
        StreamReader reader,
        int maximumCharacters,
        CancellationToken cancellationToken)
    {
        var result = new StringBuilder(Math.Min(maximumCharacters, 4096));
        var chunk = new char[2048];
        while (true)
        {
            var count = await reader.ReadAsync(chunk.AsMemory(), cancellationToken).ConfigureAwait(false);
            if (count == 0)
            {
                break;
            }

            if (result.Length + count > maximumCharacters)
            {
                throw new InvalidDataException("The agent-tool worker output exceeded its size limit.");
            }

            result.Append(chunk, 0, count);
        }

        return result.ToString();
    }

    private static async Task KillAndWaitAsync(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
            // The process may exit between HasExited and Kill.
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // Continue to the bounded wait; never leak the worker's exception details.
        }

        try
        {
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(3)).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            // A worker that cannot be reaped is still reported as timed out/failed by the caller.
        }
        catch (InvalidOperationException)
        {
            // The process was never started or has already been disposed by the OS.
        }
    }

    private sealed class WindowsWorkerJob : IDisposable
    {
        private const int JobObjectExtendedLimitInformationClass = 9;
        private const uint JobObjectLimitActiveProcess = 0x00000008;
        private const uint JobObjectLimitKillOnJobClose = 0x00002000;
        private IntPtr _handle;

        private WindowsWorkerJob(IntPtr handle) => _handle = handle;

        public static WindowsWorkerJob? CreateIfSupported()
        {
            if (!OperatingSystem.IsWindows())
            {
                return null;
            }

            var handle = CreateJobObject(IntPtr.Zero, null);
            if (handle == IntPtr.Zero)
            {
                throw new AgentToolWorkerFailedException();
            }

            var limits = new JobObjectExtendedLimitInformation
            {
                BasicLimitInformation = new JobObjectBasicLimitInformation
                {
                    LimitFlags = JobObjectLimitActiveProcess | JobObjectLimitKillOnJobClose,
                    ActiveProcessLimit = 1
                }
            };

            var informationLength = Marshal.SizeOf<JobObjectExtendedLimitInformation>();
            var information = Marshal.AllocHGlobal(informationLength);
            try
            {
                // Pass a native pointer explicitly. This also makes it possible to keep the
                // exact Win32 error if the runner rejects any requested job limit.
                Marshal.StructureToPtr(limits, information, fDeleteOld: false);
                if (!SetInformationJobObject(
                    handle,
                    JobObjectExtendedLimitInformationClass,
                    information,
                    (uint)informationLength))
                {
                    throw new AgentToolWorkerFailedException(Marshal.GetLastWin32Error());
                }

                return new WindowsWorkerJob(handle);
            }
            catch
            {
                CloseHandle(handle);
                throw;
            }
            finally
            {
                Marshal.FreeHGlobal(information);
            }
        }

        public void Assign(Process process)
        {
            if (_handle == IntPtr.Zero || !AssignProcessToJobObject(_handle, process.Handle))
            {
                throw new AgentToolWorkerFailedException();
            }
        }

        public void Dispose()
        {
            var handle = _handle;
            _handle = IntPtr.Zero;
            if (handle != IntPtr.Zero)
            {
                // JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE also terminates any surviving child process.
                CloseHandle(handle);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JobObjectBasicLimitInformation
        {
            public long PerProcessUserTimeLimit;
            public long PerJobUserTimeLimit;
            public uint LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass;
            public uint SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IoCounters
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JobObjectExtendedLimitInformation
        {
            public JobObjectBasicLimitInformation BasicLimitInformation;
            public IoCounters IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }

        [DllImport("kernel32.dll", EntryPoint = "CreateJobObjectW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateJobObject(IntPtr jobAttributes, string? name);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetInformationJobObject(
            IntPtr job,
            int informationClass,
            IntPtr information,
            uint informationLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);
    }
}

public sealed class AgentToolWorkerFailedException : InvalidOperationException
{
    public AgentToolWorkerFailedException()
        : base("The isolated agent-tool worker failed or rejected the request.")
    {
    }

    public AgentToolWorkerFailedException(int nativeErrorCode)
        : base($"The agent-tool worker could not configure its Windows process job (Win32 error {nativeErrorCode}).")
    {
        NativeErrorCode = nativeErrorCode;
    }

    public int? NativeErrorCode { get; }
}
