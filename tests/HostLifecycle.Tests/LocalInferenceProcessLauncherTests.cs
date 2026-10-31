using WhiteP.Host.Windows.Inference;

internal static class LocalInferenceProcessLauncherTests
{
    public static void Run()
    {
        var root = Path.Combine(Path.GetTempPath(), "WhiteP-ProcessLauncher-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var executable = Path.Combine(root, "llama-server.exe");
            var model = Path.Combine(root, "model file & fixture.gguf");
            File.WriteAllText(executable, "fixture only for argument construction");
            File.WriteAllText(model, "fixture only for argument construction");

            var info = LocalInferenceProcessLauncher.CreateStartInfo(executable, model, new Uri("http://127.0.0.1:8080/"), "local-model");
            Assert(!info.UseShellExecute, "The local engine must not use a shell.");
            Assert(info.CreateNoWindow, "The local engine must not open an interactive console window.");
            Assert(info.ArgumentList.Count == 9, "All process parameters must be represented by individual argument-list items.");
            Assert(info.FileName == Path.GetFullPath(executable), "The validated absolute executable path must be used.");

            var expected = new[]
            {
                "--model", Path.GetFullPath(model), "--host", "127.0.0.1",
                "--port", "8080", "--alias", "local-model", "--no-webui"
            };
            Assert(info.ArgumentList.SequenceEqual(expected),
                "Every option/value must remain separate, including a model path with spaces and shell metacharacters.");

            ExpectArgumentException(
                () => LocalInferenceProcessLauncher.CreateStartInfo(executable, model, new Uri("http://192.0.2.10:8080/"), "local-model"),
                "Non-loopback bind must be rejected.");
            ExpectArgumentException(
                () => LocalInferenceProcessLauncher.CreateStartInfo(executable, model, new Uri("http://127.0.0.1:8080/"), "model;--host=0.0.0.0"),
                "Command-like model aliases must be rejected.");
            ExpectArgumentException(
                () => LocalInferenceProcessLauncher.CreateStartInfo(Path.Combine(root, "other.exe"), model, new Uri("http://127.0.0.1:8080/"), "local-model"),
                "Other executable names must be rejected.");

            Console.WriteLine("PASS: managed process launch keeps argv separate, validates loopback and rejects unsafe values.");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static void ExpectArgumentException(Action action, string message)
    {
        try { action(); }
        catch (ArgumentException) { return; }
        throw new Exception(message);
    }
}
