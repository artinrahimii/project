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

The script publishes the actual WPF app self-contained, inventories each restored NuGet package, copies declared license files, compiles the installer, checks version/publisher metadata, then silently installs, smoke-launches, and uninstalls it in an isolated temporary directory. Third-party notices and the machine-readable license inventory ship under the installed licenses directory.

The CI artifact is a build output, not a GitHub Release publication. A public release should wait for the later release-preparation and acceptance tasks. This task does not claim a signed installer or end-to-end product readiness.

 
## Verified third-party license exception

NuGet package Collections.Pooled 1.0.82 does not ship a license expression, license URL, or license text in its restored package directory. The upstream repository identifies its LICENSE.txt as MIT. The Windows packaging script resolves the upstream repository master commit, downloads LICENSE.txt from that immutable commit, checks the MIT and copyright markers, and records the commit SHA and SHA-256 in licenses.json. This exception is scoped only to package ID/version Collections.Pooled/1.0.82; an unrecognized version or missing source file remains a hard failure.
