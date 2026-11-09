param(
    [Parameter(Mandatory = $true)]
    [string]$HostExecutable,
    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $HostExecutable -PathType Leaf)) {
    throw "The built Windows host executable does not exist: $HostExecutable"
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct WhitePWindowRect
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}

public static class WhitePWindowCapture
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr FindWindow(string className, string windowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr handle, out WhitePWindowRect rect);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr handle);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();
}
'@

$workingDirectory = Split-Path -Parent $HostExecutable
$process = Start-Process -FilePath $HostExecutable -WorkingDirectory $workingDirectory -PassThru

try {
    $windowHandle = [IntPtr]::Zero
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        $process.Refresh()
        if ($process.HasExited) {
            throw "Windows host exited before exposing its main window (exit code $($process.ExitCode))."
        }
        $windowHandle = $process.MainWindowHandle
        if ($windowHandle -ne [IntPtr]::Zero) { break }
        Start-Sleep -Milliseconds 500
    }
    if ($windowHandle -eq [IntPtr]::Zero) {
        throw "Windows host did not expose a visible main window within 30 seconds."
    }

    [void][WhitePWindowCapture]::SetForegroundWindow($windowHandle)
    Start-Sleep -Seconds 2
    if ([WhitePWindowCapture]::GetForegroundWindow() -ne $windowHandle) {
        throw "The White-P WPF window could not be brought to the foreground for native icon review."
    }

    $windowRect = New-Object WhitePWindowRect
    if (-not [WhitePWindowCapture]::GetWindowRect($windowHandle, [ref]$windowRect)) {
        throw "Could not obtain the real WPF top-level window bounds."
    }
    $windowWidth = $windowRect.Right - $windowRect.Left
    $windowHeight = $windowRect.Bottom - $windowRect.Top
    if ($windowWidth -lt 200 -or $windowHeight -lt 200) {
        throw ("WPF top-level window bounds are implausible: " + $windowWidth + "x" + $windowHeight + ".")
    }

    $taskbarHandle = [WhitePWindowCapture]::FindWindow("Shell_TrayWnd", $null)
    if ($taskbarHandle -eq [IntPtr]::Zero) {
        throw "The Windows shell taskbar (Shell_TrayWnd) is unavailable in this runner session."
    }
    $taskbarRect = New-Object WhitePWindowRect
    if (-not [WhitePWindowCapture]::GetWindowRect($taskbarHandle, [ref]$taskbarRect)) {
        throw "Could not obtain the native Windows taskbar bounds."
    }
    $taskbarWidth = $taskbarRect.Right - $taskbarRect.Left
    $taskbarHeight = $taskbarRect.Bottom - $taskbarRect.Top
    if ($taskbarWidth -lt 200 -or $taskbarHeight -lt 20) {
        throw ("Native Windows taskbar bounds are implausible: " + $taskbarWidth + "x" + $taskbarHeight + ".")
    }

    $windowImage = [System.Drawing.Bitmap]::new($windowWidth, $windowHeight)
    $windowGraphics = [System.Drawing.Graphics]::FromImage($windowImage)
    try {
        $windowGraphics.CopyFromScreen($windowRect.Left, $windowRect.Top, 0, 0, $windowImage.Size)
        $windowPath = Join-Path $OutputDirectory "windows-window-chrome.png"
        $windowImage.Save($windowPath, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $windowGraphics.Dispose()
        $windowImage.Dispose()
    }

    $taskbarImage = [System.Drawing.Bitmap]::new($taskbarWidth, $taskbarHeight)
    $taskbarGraphics = [System.Drawing.Graphics]::FromImage($taskbarImage)
    try {
        $taskbarGraphics.CopyFromScreen($taskbarRect.Left, $taskbarRect.Top, 0, 0, $taskbarImage.Size)
        $taskbarPath = Join-Path $OutputDirectory "windows-taskbar.png"
        $taskbarImage.Save($taskbarPath, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $taskbarGraphics.Dispose()
        $taskbarImage.Dispose()
    }

    foreach ($path in @($windowPath, $taskbarPath)) {
        $image = Get-Item -LiteralPath $path
        if ($image.Length -lt 1024) { throw "Native desktop review image is unexpectedly small: $path" }
        Write-Host ("Captured " + $image.Name + ": " + $image.Length + " bytes")
    }
    Write-Host "PASS: captured the visible WPF window chrome and the actual Windows shell taskbar while White-P was running."
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        [void]$process.CloseMainWindow()
        if (-not $process.WaitForExit(5000)) {
            $process.Kill()
            $process.WaitForExit()
        }
    }
    if ($null -ne $process) { $process.Dispose() }
}