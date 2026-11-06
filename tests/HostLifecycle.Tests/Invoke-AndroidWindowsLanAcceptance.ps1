[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$HostBindAddress,
    [Parameter(Mandatory = $false)]
    [ValidateRange(1, 65535)]
    [int]$Port = 43000
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
Push-Location $repoRoot

$startedAt = [DateTimeOffset]::UtcNow
$reportPath = Join-Path $repoRoot "step three/T0202/evidence/lan-acceptance.md"
$reportStatus = "FAIL"
$failureMessage = $null
$firewallRuleName = "WhiteP-T0202-LAN-$([Guid]::NewGuid().ToString('N'))"
$firewallCreated = $false
$appInstallAttempted = $false
$testInstallAttempted = $false
$serverProcess = $null
$deviceSerial = $null
$serverLog = Join-Path $env:TEMP "whitep-t0202-$([Guid]::NewGuid().ToString('N')).log"
$serverErrorLog = "$serverLog.stderr"
$sourceRevision = "unavailable"
$testClass = "com.whitep.client.network.AndroidWindowsLanIntegrationTest"

try {
    if ($env:OS -ne "Windows_NT") {
        throw "The LAN acceptance host must be Windows."
    }

    $bindIp = [System.Net.IPAddress]::Parse($HostBindAddress)
    if ($bindIp.AddressFamily -ne [System.Net.Sockets.AddressFamily]::InterNetwork -or
        [System.Net.IPAddress]::IsLoopback($bindIp) -or
        $bindIp.Equals([System.Net.IPAddress]::Any)) {
        throw "HostBindAddress must be one explicit, non-loopback IPv4 address."
    }

    $octets = $bindIp.GetAddressBytes()
    $isPrivateIpv4 = (
        $octets[0] -eq 10 -or
        ($octets[0] -eq 172 -and $octets[1] -ge 16 -and $octets[1] -le 31) -or
        ($octets[0] -eq 192 -and $octets[1] -eq 168)
    )
    if (-not $isPrivateIpv4) {
        throw "HostBindAddress must be an RFC1918 private IPv4 address."
    }

    $assigned = @(Get-NetIPAddress -AddressFamily IPv4 -ErrorAction Stop |
        Where-Object { $_.IPAddress -eq $HostBindAddress -and $_.AddressState -eq "Preferred" })
    if ($assigned.Count -eq 0) {
        throw "HostBindAddress is not assigned to an active Windows interface."
    }

    $principal = [Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "The runner must be elevated to create and remove a temporary firewall rule."
    }

    foreach ($command in @("adb", "dotnet", "gradle", "git")) {
        if (-not (Get-Command $command -ErrorAction SilentlyContinue)) {
            throw "Required command is unavailable: $command."
        }
    }
    if (-not $env:ANDROID_HOME -and -not $env:ANDROID_SDK_ROOT) {
        throw "Set ANDROID_HOME or ANDROID_SDK_ROOT on the Windows runner."
    }

    $deviceLines = @(& adb devices)
    if ($LASTEXITCODE -ne 0) {
        throw "adb devices failed."
    }
    $devices = @($deviceLines | Select-Object -Skip 1 | Where-Object { $_ -match '^\S+\s+device$' })
    if ($devices.Count -ne 1) {
        throw "Exactly one authorised Android device must be connected; found $($devices.Count)."
    }
    $deviceSerial = ($devices[0] -split '\s+')[0]
    if ($deviceSerial.StartsWith("emulator-", [StringComparison]::OrdinalIgnoreCase)) {
        throw "A physical Android device is required; an emulator on the runner is not LAN acceptance."
    }

    $existingApp = @(& adb -s $deviceSerial shell pm list packages com.whitep.client)
    if (($existingApp -join [Environment]::NewLine) -match 'package:com\.whitep\.client$') {
        throw "The test phone already has com.whitep.client installed. Existing app data will not be replaced."
    }
    $existingTestApp = @(& adb -s $deviceSerial shell pm list packages com.whitep.client.test)
    if (($existingTestApp -join [Environment]::NewLine) -match 'package:com\.whitep\.client\.test$') {
        throw "The test phone already has com.whitep.client.test installed. Existing app data will not be replaced."
    }

    $sourceRevision = (& git rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($sourceRevision)) {
        throw "Could not identify the checked-out source revision."
    }

    & dotnet build "tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj" --configuration Release
    if ($LASTEXITCODE -ne 0) {
        throw "The Windows .NET LAN fixture build failed."
    }

    Push-Location "src/Client.Android"
    try {
        & gradle --no-daemon assembleDebug assembleDebugAndroidTest
        if ($LASTEXITCODE -ne 0) {
            throw "The Android app or instrumentation APK build failed."
        }
    }
    finally {
        Pop-Location
    }

    $appApk = Join-Path $repoRoot "src/Client.Android/app/build/outputs/apk/debug/app-debug.apk"
    $testApk = Join-Path $repoRoot "src/Client.Android/app/build/outputs/apk/androidTest/debug/app-debug-androidTest.apk"
    if (-not (Test-Path $appApk) -or -not (Test-Path $testApk)) {
        throw "The expected Android APK or instrumentation APK is missing."
    }

    $appInstallAttempted = $true
    & adb -s $deviceSerial install $appApk
    if ($LASTEXITCODE -ne 0) {
        throw "Installing the Android debug APK failed."
    }

    $testInstallAttempted = $true
    & adb -s $deviceSerial install $testApk
    if ($LASTEXITCODE -ne 0) {
        throw "Installing the Android instrumentation APK failed."
    }

    $runner = "com.whitep.client.test/androidx.test.runner.AndroidJUnitRunner"
    $identityArgs = @(
        "-s", $deviceSerial, "shell", "am", "instrument", "-w",
        "-e", "class", "$testClass#publishesStableAndroidPeerIdForWindowsHostPairing",
        $runner
    )
    $identityOutput = & adb @identityArgs 2>&1
    if ($LASTEXITCODE -ne 0 -or ($identityOutput | Out-String) -notmatch 'OK\s+\(1 test\)') {
        throw "The Android Keystore identity preparation test failed."
    }

    $peerOutput = & adb -s $deviceSerial shell run-as com.whitep.client cat files/whitep-lan-peer-id.txt 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Reading the app's public PeerId through run-as failed."
    }
    $androidPeerId = ($peerOutput | Out-String).Trim()
    if ($androidPeerId -notmatch '^[1-9A-HJ-NP-Za-km-z]{20,}$') {
        throw "The Android app did not produce a canonical-looking PeerId."
    }

    $firewallParams = @{
        Name = $firewallRuleName
        DisplayName = "White-P T0202 temporary LAN TCP acceptance"
        Direction = "Inbound"
        Action = "Allow"
        Protocol = "TCP"
        LocalAddress = $HostBindAddress
        LocalPort = $Port
        RemoteAddress = "LocalSubnet"
        Profile = "Any"
    }
    New-NetFirewallRule @firewallParams | Out-Null
    $firewallCreated = $true

    $serverDll = Join-Path $repoRoot "tests/HostLifecycle.Tests/bin/Release/net10.0/HostLifecycle.Tests.dll"
    if (-not (Test-Path $serverDll)) {
        throw "The Windows peer fixture DLL is missing."
    }

    $serverArguments = '"' + $serverDll + '" --interop-peer-server-lan "' + $androidPeerId + '" "' + $Port + '" "' + $HostBindAddress + '"'
    $serverProcess = Start-Process -FilePath "dotnet" -ArgumentList $serverArguments -WorkingDirectory $repoRoot -NoNewWindow -PassThru -RedirectStandardOutput $serverLog -RedirectStandardError $serverErrorLog

    $tab = [string][char]9
    $readyLine = $null
    $deadline = [DateTimeOffset]::UtcNow.AddSeconds(45)
    while ([DateTimeOffset]::UtcNow -lt $deadline) {
        $serverProcess.Refresh()
        if ($serverProcess.HasExited) {
            throw "The Windows fixture exited before announcing its listener. Inspect the private runner process log."
        }
        if (Test-Path $serverLog) {
            $readyLine = Get-Content $serverLog | Where-Object { $_.StartsWith("READY" + $tab, [StringComparison]::Ordinal) } | Select-Object -First 1
            if ($readyLine) {
                break
            }
        }
        Start-Sleep -Seconds 1
    }
    if (-not $readyLine) {
        throw "The Windows fixture did not announce READY within 45 seconds."
    }

    $readyFields = $readyLine -split $tab
    if ($readyFields.Count -ne 3) {
        throw "The Windows fixture returned a malformed READY marker."
    }
    $windowsPeerId = $readyFields[1]
    $expectedAddress = "/ip4/$HostBindAddress/tcp/$Port/p2p/$windowsPeerId"
    if (-not $readyFields[2].Contains($expectedAddress, [StringComparison]::Ordinal)) {
        throw "The Windows fixture did not bind to the requested LAN interface."
    }

    $lanTestArgs = @(
        "-s", $deviceSerial, "shell", "am", "instrument", "-w",
        "-e", "class", "$testClass#connectsToWindowsHostOverNonLoopbackLan",
        "-e", "host_peer_id", $windowsPeerId,
        "-e", "host_ip", $HostBindAddress,
        "-e", "host_port", "$Port",
        $runner
    )
    $testOutput = & adb @lanTestArgs 2>&1
    if ($LASTEXITCODE -ne 0 -or ($testOutput | Out-String) -notmatch 'OK\s+\(1 test\)') {
        throw "The Android-to-Windows LAN instrumentation test failed."
    }

    if (-not $serverProcess.WaitForExit(15000)) {
        throw "The Windows fixture did not observe the Android peer stream closing."
    }
    if ($serverProcess.ExitCode -ne 0) {
        throw "The Windows fixture reported a failed request/response exchange."
    }
    $serverLogText = if (Test-Path $serverLog) { Get-Content $serverLog -Raw } else { "" }
    if ($serverLogText -notmatch "(?m)^PASS\s*$") {
        throw "The Windows fixture did not record its final PASS marker."
    }

    $reportStatus = "PASS"
    Write-Host "T0202 LAN ACCEPTANCE: PASS"
}
catch {
    $failureMessage = $_.Exception.Message
    throw
}
finally {
    if ($serverProcess) {
        try {
            $serverProcess.Refresh()
            if (-not $serverProcess.HasExited) {
                Stop-Process -Id $serverProcess.Id -Force -ErrorAction SilentlyContinue
            }
        }
        catch { }
    }

    if ($firewallCreated) {
        try {
            Remove-NetFirewallRule -Name $firewallRuleName -ErrorAction SilentlyContinue
        }
        catch { }
    }

    $endedAt = [DateTimeOffset]::UtcNow
    $reportLines = @(
        "# T0202 — Android-to-Windows LAN acceptance result",
        "",
        "- Result: **$reportStatus**",
        "- Source revision: $sourceRevision",
        "- Started: $($startedAt.ToString('O'))",
        "- Ended: $($endedAt.ToString('O'))",
        "- Test: $testClass connectsToWindowsHostOverNonLoopbackLan",
        "- Scope: production Android libp2p TCP/Noise transport to the Windows .NET libp2p transport on an explicitly assigned RFC1918 interface.",
        "- Checks: authenticated peer identity, request delivery, correlated response and peer-stream close.",
        "- Privacy: no prompt/response payload, PeerId, private IP address or private key is recorded in this report."
    )
    if ($failureMessage) {
        $reportLines += ""
        $reportLines += "- Failure summary: $failureMessage"
    }

    $reportDirectory = Split-Path -Parent $reportPath
    if (-not (Test-Path $reportDirectory)) {
        New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
    }
    $reportLines | Set-Content -Path $reportPath -Encoding utf8

    # Only packages installed by this invocation are removed after the initial absence check.
    if ($deviceSerial -and $testInstallAttempted) {
        try {
            & adb -s $deviceSerial uninstall com.whitep.client.test | Out-Null
        }
        catch { }
    }
    if ($deviceSerial -and $appInstallAttempted) {
        try {
            & adb -s $deviceSerial uninstall com.whitep.client | Out-Null
        }
        catch { }
    }

    Pop-Location
}
