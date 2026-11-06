# Windows installer

The Windows host is packaged as a per-user x64 installer with Inno Setup 6. The package is built from a self-contained dotnet publish output, so a separate .NET Desktop Runtime installation is not required.

- Product: White-P
- Version: 0.1.0, aligned with the Android client's initial version.
- Publisher metadata: Artin Rahimi.
- Architecture: win-x64.
- Default install directory: %LOCALAPPDATA%\Programs\White-P.
- Output: artifacts/releases/WhiteP-Setup-0.1.0-win-x64.exe.

Publisher and version are descriptive installer metadata. The installer is unsigned because no trusted Authenticode certificate/private key is provisioned for CI. The displayed publisher is not a Windows-trusted digital signature.

On Windows x64 with .NET SDK 10.0.100, Python 3.13, and Inno Setup 6:

    python -m unittest discover -s tests -p test_generate_dependency_licenses.py -v
    .\tools\build_windows_installer.ps1

The script publishes the actual WPF app self-contained, inventories each restored NuGet package, copies declared license files, compiles the installer, checks its binary product version, then silently installs it in an isolated temporary directory. It validates the publisher using the actual per-user uninstall registration matched to that isolated install path, verifies the notices, smoke-launches the installed app, uninstalls it, and confirms the matching uninstall registration is removed. The setup executable's PE CompanyName field is logged as a separate diagnostic because it may differ from the AppPublisher registration. Third-party notices and the machine-readable license inventory ship under the installed licenses directory.

## Exact-version third-party license sources

Some restored NuGet versions do not include license metadata. The third_party_license_overrides.json file keeps narrowly scoped overrides by package ID and exact version. The build resolves each upstream repository's branch to a complete Git commit SHA, downloads the actual license file from that immutable revision, checks identifying license markers, and records the source revision and SHA-256 digest in the generated inventory.

- Collections.Pooled 1.0.82: upstream MIT license file from jtmueller/Collections.Pooled.
- Common.Logging 3.4.1 and Common.Logging.Core 3.4.1: upstream Apache-2.0 license file from net-commons/common-logging. The Core package is the core implementation of the same Common.Logging project.
- Makaretu.Dns 2.0.1: upstream MIT license file from richardschneider/net-dns.
- Makaretu.Dns.Multicast 0.27.0: upstream MIT license file from richardschneider/net-mdns.

The Makaretu package READMEs identify their corresponding NuGet package IDs and explicitly declare the MIT license. The installer build resolves each repository's current master revision to a full Git SHA, downloads LICENSE from that immutable revision, verifies the MIT and copyright markers, and records the revision and SHA-256 in the generated notices.

A missing or changed upstream file, an invalid revision, an unrecognized package version, or any remaining package without verified license metadata fails the build. Missing package entries are collected and reported together so maintainers can address them without weakening the fail-closed behavior.

The CI artifact is a build output, not a GitHub Release publication. A public release should wait for the later release-preparation and acceptance tasks. This task does not claim a signed installer or end-to-end product readiness.


## Sample model license notice

The Windows installer also includes licenses/ModelLicenses.md and a machine-readable licenses/model-licenses.json. These are generated from src/Host.Windows/Models/Catalog/sample-model-catalog.json and include the declared model identifier, model-card URL, expected model-file SHA-256, the full official Apache-2.0 license text under licenses/files/WhiteP-Sample-Model-Apache-2.0.txt, and the SHA-256 digest of that text. The build downloads the license from https://www.apache.org/licenses/LICENSE-2.0.txt and rejects an unexpectedly short, oversized, non-UTF-8, or marker-incomplete response.

The checked-in sample model card declares Apache-2.0; the catalog omits classification, so the notice records it as unknown instead of inferring a category. The model weights themselves are not included in the installer. This notice records upstream metadata and the referenced license text; it is not legal advice or an independent audit of all rights in the quantized and base models. Review the upstream model card and original base-model terms before any separate redistribution of model weights.


The sample model catalog digest is computed from UTF-8 content after normalizing CRLF pairs to LF. This keeps the recorded SHA-256 stable when Git converts line endings in a Windows working tree; bare CR line endings are rejected.
