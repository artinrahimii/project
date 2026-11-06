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

function Get-VerifiedUpstreamLicense {
    param(
        [string]$Repository,
        [string]$Branch,
        [string]$RelativeLicensePath,
        [string]$OutputFile,
        [string]$RevisionFile,
        [string[]]$RequiredMarkers
    )
    $revisionLine = & git ls-remote --refs $Repository "refs/heads/$Branch"
    if ($LASTEXITCODE -ne 0 -or -not $revisionLine) {
        throw "Could not resolve upstream license source revision from $Repository."
    }
    $revision = ($revisionLine -split "\s+")[0]
    if ($revision -notmatch "^[0-9a-f]{40}$") {
        throw "Upstream license source revision is not a full Git SHA-1: $Repository"
    }
    $repositoryPath = $Repository.Replace("https://github.com/", "").Replace(".git", "")
    $url = "https://raw.githubusercontent.com/$repositoryPath/$revision/$RelativeLicensePath"
    Invoke-WebRequest -Uri $url -OutFile $OutputFile -TimeoutSec 60
    $file = Get-Item -LiteralPath $OutputFile
    if ($file.Length -lt 500 -or $file.Length -gt 131072) {
        throw "Upstream license file has an unexpected size: $url"
    }
    $contents = [IO.File]::ReadAllText($OutputFile)
    foreach ($marker in $RequiredMarkers) {
        if (-not $contents.Contains($marker)) {
            throw "Upstream license file is missing a required marker '$marker': $url"
        }
    }
    [IO.File]::WriteAllText($RevisionFile, $revision, [Text.Encoding]::ASCII)
    Write-Host "Verified upstream license source revision: $repositoryPath $revision"
}

Get-VerifiedUpstreamLicense -Repository "https://github.com/jtmueller/Collections.Pooled.git" -Branch "master" -RelativeLicensePath "LICENSE.txt" -OutputFile (Join-Path $licenseSources "Collections.Pooled-1.0.82-LICENSE.txt") -RevisionFile (Join-Path $licenseSources "Collections.Pooled.source-revision.txt") -RequiredMarkers @("The MIT License (MIT)", "Copyright (c) Joel Mueller")
Get-VerifiedUpstreamLicense -Repository "https://github.com/net-commons/common-logging.git" -Branch "master" -RelativeLicensePath "license.txt" -OutputFile (Join-Path $licenseSources "common-logging-license.txt") -RevisionFile (Join-Path $licenseSources "Common.Logging.source-revision.txt") -RequiredMarkers @("Apache License", "Version 2.0", "http://www.apache.org/licenses/LICENSE-2.0")
Get-VerifiedUpstreamLicense -Repository "https://github.com/richardschneider/net-dns.git" -Branch "master" -RelativeLicensePath "LICENSE" -OutputFile (Join-Path $licenseSources "Makaretu.Dns-2.0.1-LICENSE.txt") -RevisionFile (Join-Path $licenseSources "Makaretu.Dns.source-revision.txt") -RequiredMarkers @("MIT License", "Copyright (c) 2018 Richard Schneider")
Get-VerifiedUpstreamLicense -Repository "https://github.com/richardschneider/net-mdns.git" -Branch "master" -RelativeLicensePath "LICENSE" -OutputFile (Join-Path $licenseSources "Makaretu.Dns.Multicast-0.27.0-LICENSE.txt") -RevisionFile (Join-Path $licenseSources "Makaretu.Dns.Multicast.source-revision.txt") -RequiredMarkers @("MIT License", "Copyright (c) 2018 Richard Schneider")

$assets = Join-Path $root "src\Host.Windows\obj\project.assets.json"
if (-not (Test-Path -LiteralPath $assets -PathType Leaf)) { throw "NuGet restore assets are missing." }
& python (Join-Path $root "tools\generate_dependency_licenses.py") --assets $assets --output-dir $licenses --overrides $overrides --license-source-dir $licenseSources
if ($LASTEXITCODE -ne 0) { throw "Dependency license inventory failed: $LASTEXITCODE" }
$notice = Join-Path $licenses "ThirdPartyNotices.md"
$manifest = Join-Path $licenses "licenses.json"
if (-not (Test-Path -LiteralPath $notice -PathType Leaf) -or -not (Test-Path -LiteralPath $manifest -PathType Leaf)) { throw "Dependency notices were not generated." }
$licenseData = Get-Content -LiteralPath $manifest -Raw | ConvertFrom-Json
if ($licenseData.package_count -lt 1) { throw "No NuGet packages were inventoried." }

$sampleModelCatalog = Join-Path $root "src\Host.Windows\Models\Catalog\sample-model-catalog.json"
$apacheLicensePath = Join-Path $licenseSources "Apache-2.0.txt"
if (-not (Test-Path -LiteralPath $sampleModelCatalog -PathType Leaf)) {
    throw "Sample model catalog is missing: $sampleModelCatalog"
}
Invoke-WebRequest -Uri "https://www.apache.org/licenses/LICENSE-2.0.txt" -OutFile $apacheLicensePath -TimeoutSec 60
$apacheLicenseInfo = Get-Item -LiteralPath $apacheLicensePath
if ($apacheLicenseInfo.Length -lt 1024 -or $apacheLicenseInfo.Length -gt 131072) {
    throw "Official Apache-2.0 license text has an unexpected size: $($apacheLicenseInfo.Length) bytes."
}
& python (Join-Path $root "tools\generate_model_license_notice.py") --catalog $sampleModelCatalog --apache-license-file $apacheLicensePath --output-dir $licenses
if ($LASTEXITCODE -ne 0) { throw "Sample model license notice generation failed: $LASTEXITCODE" }
$modelNotice = Join-Path $licenses "ModelLicenses.md"
$modelManifest = Join-Path $licenses "model-licenses.json"
$modelLicenseText = Join-Path $licenses "files\WhiteP-Sample-Model-Apache-2.0.txt"
foreach ($path in @($modelNotice, $modelManifest, $modelLicenseText)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Sample model license output is missing: $path" }
}
$modelLicenseData = Get-Content -LiteralPath $modelManifest -Raw | ConvertFrom-Json
if ($modelLicenseData.model_count -lt 1) { throw "No sample model license was inventoried." }

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
$setupCompanyName = [string]$setupInfo.CompanyName
if ($setupCompanyName -ne $Publisher) {
    Write-Warning "Installer PE CompanyName metadata was '$setupCompanyName'; expected '$Publisher'. The installed AppPublisher registration is checked separately below."
}

try {
    $install = Start-Process -FilePath $installer -ArgumentList @("/VERYSILENT", "/SUPPRESSMSGBOXES", "/NORESTART", "/SP-", "/DIR=$installDir") -Wait -PassThru
    if ($install.ExitCode -ne 0) { throw "Silent install failed: $($install.ExitCode)" }
    $installedExe = Join-Path $installDir "WhiteP.Host.Windows.exe"
    if (-not (Test-Path -LiteralPath $installedExe -PathType Leaf)) { throw "Installed application executable is missing." }
    if (-not (Test-Path -LiteralPath (Join-Path $installDir "licenses\ThirdPartyNotices.md") -PathType Leaf)) { throw "Third-party notices are missing from installation." }
    if (-not (Test-Path -LiteralPath (Join-Path $installDir "licenses\ModelLicenses.md") -PathType Leaf)) { throw "Sample model license notice is missing from installation." }
    if (-not (Test-Path -LiteralPath (Join-Path $installDir "licenses\files\WhiteP-Sample-Model-Apache-2.0.txt") -PathType Leaf)) { throw "Sample model Apache-2.0 license text is missing from installation." }
    $expectedInstallLocation = [IO.Path]::GetFullPath($installDir).TrimEnd([char[]]@('\', '/'))
    $publisherRegistrations = @(
        Get-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*" -ErrorAction SilentlyContinue |
            Where-Object {
                if ($_.DisplayName -ne "White-P" -or [string]::IsNullOrWhiteSpace([string]$_.InstallLocation)) {
                    return $false
                }
                try {
                    $registeredInstallLocation = [IO.Path]::GetFullPath([string]$_.InstallLocation).TrimEnd([char[]]@('\', '/'))
                    [string]::Equals($registeredInstallLocation, $expectedInstallLocation, [StringComparison]::OrdinalIgnoreCase)
                }
                catch {
                    $false
                }
            }
    )
    if ($publisherRegistrations.Count -ne 1) {
        throw "Expected exactly one White-P uninstall registration for the isolated install location; found $($publisherRegistrations.Count)."
    }
    $registeredPublisher = [string]$publisherRegistrations[0].Publisher
    if ($registeredPublisher -ne $Publisher) {
        throw "Installed White-P publisher registration mismatch: expected '$Publisher', found '$registeredPublisher'."
    }
    Write-Host "PASS: installed White-P registration reports publisher '$registeredPublisher' for the isolated install location."
    $smoke = Start-Process -FilePath $installedExe -ArgumentList "--smoke-test" -WorkingDirectory $installDir -Wait -PassThru
    if ($smoke.ExitCode -ne 0) { throw "Installed application smoke test failed: $($smoke.ExitCode)" }
    $uninstaller = Join-Path $installDir "unins000.exe"
    if (-not (Test-Path -LiteralPath $uninstaller -PathType Leaf)) { throw "Uninstaller was not created." }
    $uninstall = Start-Process -FilePath $uninstaller -ArgumentList @("/VERYSILENT", "/SUPPRESSMSGBOXES", "/NORESTART") -Wait -PassThru
    if ($uninstall.ExitCode -ne 0) { throw "Silent uninstall failed: $($uninstall.ExitCode)" }
    if (Test-Path -LiteralPath $installDir) { throw "Uninstall did not remove the isolated installation directory." }
    $remainingPublisherRegistrations = @(
        Get-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*" -ErrorAction SilentlyContinue |
            Where-Object {
                $_.DisplayName -eq "White-P" -and
                -not [string]::IsNullOrWhiteSpace([string]$_.InstallLocation) -and
                [string]::Equals(
                    [IO.Path]::GetFullPath([string]$_.InstallLocation).TrimEnd([char[]]@('\', '/')),
                    $expectedInstallLocation,
                    [StringComparison]::OrdinalIgnoreCase
                )
            }
    )
    if ($remainingPublisherRegistrations.Count -ne 0) {
        throw "The White-P uninstall registration remained after uninstall."
    }
}
finally {
    if (Test-Path -LiteralPath $installDir) { Remove-Item -LiteralPath $installDir -Recurse -Force -ErrorAction SilentlyContinue }
}
Write-Host "PASS: installer compiled, installed, smoke-launched, included notices, and uninstalled."
Write-Host "Installer: $installer ($($installerInfo.Length) bytes); NuGet packages: $($licenseData.package_count)"
