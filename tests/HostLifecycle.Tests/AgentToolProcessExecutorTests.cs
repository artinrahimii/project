using System.Text.Json;
using WhiteP.Host.Windows.Chat;
using WhiteP.Host.Windows.Tools;

internal static class AgentToolProcessExecutorTests
{
    public static async Task RunAsync()
    {
        var executor = AgentToolExecutionAcceptanceTests.CreateProcessExecutor(TimeSpan.FromSeconds(15));
        var startInfo = executor.CreateStartInfo();
        Assert(!startInfo.UseShellExecute, "The worker must never run through a shell.");
        Assert(startInfo.CreateNoWindow, "The worker must not open an interactive console window.");
        Assert(startInfo.RedirectStandardInput && startInfo.RedirectStandardOutput && startInfo.RedirectStandardError,
            "Worker protocol streams must be redirected and bounded.");
        Assert(Path.IsPathFullyQualified(startInfo.FileName), "The worker must use a trusted absolute executable path.");
        Assert(startInfo.ArgumentList.Last() == "--agent-tool-worker",
            "Only the fixed worker entrypoint flag may be appended to trusted worker arguments.");
        Assert(!startInfo.Environment.ContainsKey("PATH") &&
            !startInfo.Environment.ContainsKey("WHITEP_AGENT_TOOL_SECRET_CANARY"),
            "The worker must not inherit PATH or arbitrary caller secrets.");

        var previousSecret = Environment.GetEnvironmentVariable("WHITEP_AGENT_TOOL_SECRET_CANARY");
        try
        {
            Environment.SetEnvironmentVariable("WHITEP_AGENT_TOOL_SECRET_CANARY", "never-copy-this-secret");
            var result = await executor.ExecuteAsync(CreateCall("get_host_status", "{}"));
            Assert(!result.IsError && result.Content == "Executed get_host_status.",
                "A successful allowlisted worker response must round-trip without leaking parent environment variables.");
        }
        finally
        {
            Environment.SetEnvironmentVariable("WHITEP_AGENT_TOOL_SECRET_CANARY", previousSecret);
        }

        var started = DateTime.UtcNow;
        var timeoutExecutor = AgentToolExecutionAcceptanceTests.CreateProcessExecutor(TimeSpan.FromSeconds(2));
        var timedOut = false;
        try
        {
            _ = await timeoutExecutor.ExecuteAsync(CreateCall("hang_tool", "{}"));
        }
        catch (TimeoutException)
        {
            timedOut = true;
        }

        var elapsed = DateTime.UtcNow - started;
        Assert(timedOut, "A non-terminating tool must be stopped by the enforced worker timeout.");
        Assert(elapsed < TimeSpan.FromSeconds(10),
            "A timed-out worker must be killed/reaped promptly rather than leaving the call pending.");

        var oversizedRejected = false;
        using (var json = JsonDocument.Parse(JsonSerializer.Serialize(new { value = new string('x', 70_000) })))
        {
            try
            {
                _ = await executor.ExecuteAsync(new AgentToolCall("get_host_status", json.RootElement));
            }
            catch (InvalidDataException)
            {
                oversizedRejected = true;
            }
        }

        Assert(oversizedRejected, "Oversized model arguments must be rejected before launching a process.");

        var shellNameRejected = false;
        try
        {
            _ = new AgentToolProcessExecutor(Path.Combine(
                Path.GetDirectoryName(startInfo.FileName)!,
                OperatingSystem.IsWindows() ? "cmd.exe" : "bash"));
        }
        catch (ArgumentException)
        {
            shellNameRejected = true;
        }

        Assert(shellNameRejected, "A command shell must not be accepted as the trusted tool-worker executable.");
        Console.WriteLine("PASS: agent tools execute in a bounded child process with a sanitized environment, strict protocol limits, shell-free launch and enforced timeout.");
    }

    private static AgentToolCall CreateCall(string name, string json)
    {
        using var document = JsonDocument.Parse(json);
        return new AgentToolCall(name, document.RootElement);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
