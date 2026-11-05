[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64",
    [string]$Version = "0.1.0",
    [string]$Publisher = "Artin Rahimi",
    [string]$IsccPath = ""
)
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$project = Join-Path $root "src\Host.Windows\WhiteP.Host.Windows.csproj"
$publish = Join-Path $root "artifacts\windows-installer\publish"
$licenses = Join-Path $root "artifacts\windows-installer\licenses"
$licenseSources = Join-Path $root "artifacts\windows-installer\license-sources"
$output = Join-Path $root "artifacts\releases"
$script = Join-Path $root "tools\installer\WhiteP.iss"
$overrides = Join-Path $root "tools\third_party_license_overrides.json"
$installer = Join-Path $output "WhiteP-Setup-$Version-$RuntimeIdentifier.exe"
$installDir = Join-Path $env:TEMP ("WhiteP-Installer-" + [guid]::NewGuid().ToString("N"))
if ($RuntimeIdentifier -ne "win-x64") { throw "Only win-x64 is supported." }
if (-not $IsccPath) {
    $programFiles = [Environment]::GetFolderPath("ProgramFilesX86")
    $IsccPath = Join-Path $programFiles "Inno Setup 6\ISCC.exe"
}
foreach ($path in @($project, $script, $overrides, $IsccPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Required input is missing: $path" }
}
foreach ($path in @($publish, $licenses, $licenseSources)) {
    if (Test-Path -LiteralPath $path) { Remove-Item -LiteralPath $path -Recurse -Force }
    New-Item -ItemType Directory -Path $path -Force | Out-Null
}
New-Item -ItemType Directory -Path $output -Force | Out-Null
if (Test-Path -LiteralPath $installer) { Remove-Item -LiteralPath $installer -Force }

$publishArgs = @(
    "publish", $project, "--configuration", $Configuration, "--runtime", $RuntimeIdentifier,
    "--self-contained", "true", "--output", $publish,
    "-p:Version=$Version", "-p:AssemblyVersion=$Version.0", "-p:FileVersion=$Version.0",
    "-p:InformationalVersion=$Version", "-p:Company=$Publisher", "-p:Authors=$Publisher", "-p:Product=White-P"
)
& dotnet @publishArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed: $LASTEXITCODE" }
$exe = Join-Path $publish "WhiteP.Host.Windows.exe"
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) { throw "Published WPF executable is missing." }
$versionInfo = [Diagnostics.FileVersionInfo]::GetVersionInfo($exe)
if (-not $versionInfo.ProductVersion.StartsWith($Version, [StringComparison]::Ordinal)) { throw "Published executable version mismatch." }
if ($versionInfo.CompanyName -ne $Publisher) { throw "Published executable publisher mismatch." }

# Resolve an immutable upstream source revision, then fetch that exact license file.
$upstreamRepository = "https://github.com/jtmueller/Collections.Pooled.git"
$revisionLine = & git ls-remote --refs $upstreamRepository "refs/heads/master"
if ($LASTEXITCODE -ne 0 -or -not $revisionLine) { throw "Could not resolve upstream Collections.Pooled license source revision." }
$revision = ($revisionLine -split "\s+")[0]
if ($revision -notmatch "^[0-9a-f]{40}$") { throw "Upstream license source revision is not a full Git SHA-1." }
$licenseUrl = "https://raw.githubusercontent.com/jtmueller/Collections.Pooled/$revision/LICENSE.txt"
$licenseSource = Join-Path $licenseSources "Collections.Pooled-1.0.82-LICENSE.txt"
Invoke-WebRequest -Uri $licenseUrl -OutFile $licenseSource -TimeoutSec 60
$licenseInfo = Get-Item -LiteralPath $licenseSource
if ($licenseInfo.Length -lt 500 -or $licenseInfo.Length -gt 16384) { throw "Upstream Collections.Pooled license file has an unexpected size." }
$licenseText = [IO.File]::ReadAllText($licenseSource)
if (-not $licenseText.Contains("The MIT License (MIT)") -or -not $licenseText.Contains("Copyright (c) Joel Mueller")) {
    throw "Upstream Collections.Pooled license file did not match the expected MIT notice markers."
}
Write-Host "Verified Collections.Pooled license source revision: $revision"

$assets = Join-Path $root "src\Host.Windows\obj\project.assets.json"
if (-not (Test-Path -LiteralPath $assets -PathType Leaf)) { throw "NuGet restore assets are missing." }
& python (Join-Path $root "tools\generate_dependency_licenses.py") --assets $assets --output-dir $licenses --overrides $overrides --license-source-dir $licenseSources --license-source-revision $revision
if ($LASTEXITCODE -ne 0) { throw "Dependency license inventory failed: $LASTEXITCODE" }
$notice = Join-Path $licenses "ThirdPartyNotices.md"
$manifest = Join-Path $licenses "licenses.json"
if (-not (Test-Path -LiteralPath $notice -PathType Leaf) -or -not (Test-Path -LiteralPath $manifest -PathType Leaf)) { throw "Dependency notices were not generated." }
$licenseData = Get-Content -LiteralPath $manifest -Raw | ConvertFrom-Json
if ($licenseData.package_count -lt 1) { throw "No NuGet packages were inventoried." }

$compileArgs = @(
    "/Q", "/DAppVersion=$Version", "/DPublisher=$Publisher",
    "/DPublishDir=$publish", "/DLicensesDir=$licenses", "/DOutputDir=$output", $script
)
& $IsccPath @compileArgs
if ($LASTEXITCODE -ne 0) { throw "Inno Setup compilation failed: $LASTEXITCODE" }
if (-not (Test-Path -LiteralPath $installer -PathType Leaf)) { throw "Installer output is missing." }
$installerInfo = Get-Item -LiteralPath $installer
if ($installerInfo.Length -lt 10MB) { throw "Installer is unexpectedly small: $($installerInfo.Length) bytes." }
$setupInfo = [Diagnostics.FileVersionInfo]::GetVersionInfo($installer)
if (-not $setupInfo.ProductVersion.StartsWith($Version, [StringComparison]::Ordinal)) { throw "Installer version metadata mismatch." }
if ($setupInfo.CompanyName -ne $Publisher) { throw "Installer publisher metadata mismatch." }

try {
    $install = Start-Process -FilePath $installer -ArgumentList @("/VERYSILENT", "/SUPPRESSMSGBOXES", "/NORESTART", "/SP-", "/DIR=$installDir") -Wait -PassThru
    if ($install.ExitCode -ne 0) { throw "Silent install failed: $($install.ExitCode)" }
    $installedExe = Join-Path $installDir "WhiteP.Host.Windows.exe"
    if (-not (Test-Path -LiteralPath $installedExe -PathType Leaf)) { throw "Installed application executable is missing." }
    if (-not (Test-Path -LiteralPath (Join-Path $installDir "licenses\ThirdPartyNotices.md") -PathType Leaf)) { throw "Third-party notices are missing from installation." }
    $smoke = Start-Process -FilePath $installedExe -ArgumentList "--smoke-test" -WorkingDirectory $installDir -Wait -PassThru
    if ($smoke.ExitCode -ne 0) { throw "Installed application smoke test failed: $($smoke.ExitCode)" }
    $uninstaller = Join-Path $installDir "unins000.exe"
    if (-not (Test-Path -LiteralPath $uninstaller -PathType Leaf)) { throw "Uninstaller was not created." }
    $uninstall = Start-Process -FilePath $uninstaller -ArgumentList @("/VERYSILENT", "/SUPPRESSMSGBOXES", "/NORESTART") -Wait -PassThru
    if ($uninstall.ExitCode -ne 0) { throw "Silent uninstall failed: $($uninstall.ExitCode)" }
    if (Test-Path -LiteralPath $installDir) { throw "Uninstall did not remove the isolated installation directory." }
}
finally {
    if (Test-Path -LiteralPath $installDir) { Remove-Item -LiteralPath $installDir -Recurse -Force -ErrorAction SilentlyContinue }
}
Write-Host "PASS: installer compiled, installed, smoke-launched, included notices, and uninstalled."
Write-Host "Installer: $installer ($($installerInfo.Length) bytes); NuGet packages: $($licenseData.package_count)"
Write-Host "Collections.Pooled license SHA-256: $((Get-FileHash -LiteralPath (Join-Path $licenses 'files\Collections.Pooled-1.0.82-LICENSE.txt') -Algorithm SHA256).Hash)"
