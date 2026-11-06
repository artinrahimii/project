using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace WhiteP.Host.Windows.Inference;

public static class LocalInferenceBinaryVerifier
{
    private static readonly Regex Sha256Pattern = new("^[0-9a-fA-F]{64}$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static LocalInferenceBinaryVerification Verify(string executablePath, string expectedVersion, string expectedSha256, TimeSpan? versionProbeTimeout = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedVersion);
        if (expectedVersion.Length > 128 || expectedVersion.Any(char.IsControl)) throw new ArgumentException("A valid expected engine version is required.", nameof(expectedVersion));
        if (string.IsNullOrWhiteSpace(expectedSha256) || !Sha256Pattern.IsMatch(expectedSha256.Trim())) throw new ArgumentException("Expected engine SHA-256 must be exactly 64 hexadecimal characters.", nameof(expectedSha256));
        var fullPath = Path.GetFullPath(executablePath.Trim());
        if (!File.Exists(fullPath) || !string.Equals(Path.GetFileName(fullPath), "llama-server.exe", StringComparison.OrdinalIgnoreCase)) throw new FileNotFoundException("Select an existing llama-server.exe executable.", fullPath);
        var expectedHash = Convert.FromHexString(expectedSha256.Trim());
        var actualHash = ComputeSha256(fullPath);
        if (!CryptographicOperations.FixedTimeEquals(expectedHash, actualHash)) throw new InvalidDataException("The llama.cpp executable SHA-256 does not match the expected trusted value.");
        var timeout = versionProbeTimeout ?? TimeSpan.FromSeconds(5);
        if (timeout <= TimeSpan.Zero || timeout > TimeSpan.FromMinutes(1)) throw new ArgumentOutOfRangeException(nameof(versionProbeTimeout));
        var startInfo = new ProcessStartInfo { FileName = fullPath, WorkingDirectory = Path.GetDirectoryName(fullPath)!, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        startInfo.ArgumentList.Add("--version");
        using var process = new Process { StartInfo = startInfo };
        try
        {
            if (!process.Start()) throw new InvalidOperationException("The verified llama.cpp executable did not start its version probe.");
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            using var timeoutSource = new CancellationTokenSource(timeout);
            try { process.WaitForExitAsync(timeoutSource.Token).GetAwaiter().GetResult(); }
            catch (OperationCanceledException) { try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { } throw new TimeoutException("The llama.cpp version probe timed out."); }
            var output = (stdout.GetAwaiter().GetResult() + Environment.NewLine + stderr.GetAwaiter().GetResult()).Trim();
            if (process.ExitCode != 0) throw new InvalidDataException("The llama.cpp executable failed its --version probe.");
            if (!output.Contains(expectedVersion.Trim(), StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("The llama.cpp executable version output does not match the expected version.");
            actualHash = ComputeSha256(fullPath);
            if (!CryptographicOperations.FixedTimeEquals(expectedHash, actualHash)) throw new InvalidDataException("The llama.cpp executable changed during version verification.");
            return new LocalInferenceBinaryVerification(fullPath, expectedVersion.Trim(), Convert.ToHexString(actualHash).ToLowerInvariant(), output);
        }
        catch (System.ComponentModel.Win32Exception exception) { throw new InvalidOperationException("Windows could not execute the llama.cpp version probe.", exception); }
    }

    private static byte[] ComputeSha256(string path) { using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read); return SHA256.HashData(stream); }
}

public sealed record LocalInferenceBinaryVerification(string ExecutablePath, string Version, string Sha256, string VersionOutput);
