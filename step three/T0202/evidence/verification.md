# T0202 — verification evidence

## Source revision inspected

- Repository: `artinrahimii/project`
- Branch: `main`
- Revision: `ef66bdcae307fd2324fb33cb112bbdfdffb6d196` (`V 3.0.923`)

## Existing partial evidence (not T0202 acceptance)

- `src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt`: JVM tests launch .NET fixture processes locally; this does not prove the Android app on a device/emulator can connect to a Windows host over a LAN.
- `tests/HostLifecycle.Tests/AndroidInteropPeerServer.cs`: fixture binds to `IPAddress.Loopback`.
- `src/Host.Windows/Networking/Libp2pPeerTransport.cs`: listening requires a specific interface address; wildcard binding is rejected.
- `.github/workflows/product-build.yml`: Windows WPF host and Linux Android jobs are separate. The Android instrumentation step runs the Keystore/Room acceptance suite.
- [Product Build run 41886746018](https://github.com/artinrahimii/project/actions/runs/41886746018): Windows build, Android JVM tests and nine Keystore/Room instrumentation tests succeeded, but these checks do not establish the requested LAN path.

## Status

The evidence establishes the gap, not successful T0202 acceptance. The task remains blocked until a real Android-to-Windows local-network request/response test passes.

## T0202-A002 — executable LAN harness review (not acceptance)

- Base: 95eeb40ed96ff6ef1950beb0746e6a30f761f79d (`V 3.0.925`), branch `main`.
- The Windows .NET fixture now has an opt-in non-loopback IPv4 listener mode while retaining its original loopback default for JVM/.NET tests.
- Added `src/Client.Android/app/src/androidTest/java/com/whitep/client/network/AndroidWindowsLanIntegrationTest.kt`. It derives the Android PeerId from the Keystore-backed identity, explicitly trusts the Windows PeerId for the test, dials a private non-loopback IPv4 TCP multiaddress through the production Android transport, and validates request, attempt, round, model and peer correlation in the returned response.
- Added `tests/HostLifecycle.Tests/Invoke-AndroidWindowsLanAcceptance.ps1` and `.github/workflows/android-windows-lan-acceptance.yml`. The workflow is manual-only and requires a Windows runner labelled `whitep-lan` with one physical Android device connected to the same LAN.
- Added `step three/T0202/lan-test-runbook.md` with runner, device, SDK, firewall, invocation and evidence rules.
- Static source preflight passed for the non-loopback/interface guard, production transport usage, explicit trust/revocation path, manual runner labels and temporary firewall cleanup.

### Not executed

- `dotnet build tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release` — not run; `dotnet` is unavailable in this Linux authoring runtime.
- `gradle --no-daemon assembleDebug assembleDebugAndroidTest` — not run; Gradle is unavailable here.
- `adb` installation and Android instrumentation — not run; ADB and a physical handset are unavailable here.
- End-to-end authenticated Android-to-Windows request/response on the LAN — not run. No LAN pass or workflow artifact is claimed.

**Status remains blocked.** A passing manual workflow run on a physical device and independent Windows host is required before T0202 can be marked done.

## PowerShell runner Blob correction

- The previous runner file was found to contain duplicated trailing code when read back from the committed Blob.
- Replaced it with a fresh, independently created Blob and fetched that Blob again.
- Verified exact character-for-character round trip, 293 lines, one script entry point, one firewall-rule creation, one Windows fixture process launch and expected cleanup structure.
- This source correction is not an end-to-end LAN test and does not change T0202's blocked status.

## CI result and correction — T0202-A003

- [White-P product build run 41899975472](https://github.com/artinrahimii/project/actions/runs/41899975472), Android job 126819471978.
- The Windows .NET peer fixture build completed before Android lint.
- `gradle --no-daemon ktlintCheck` failed at AndroidWindowsLanIntegrationTest.kt line 118 with `Expected newline before '.'` for the InferenceRequest builder chain.
- The builder chain is corrected by moving `.newBuilder()` to the next line. The Android APK build and instrumentation were skipped in that failed run; no Android test pass is claimed.
- A fresh CI run is required on the correction commit. The physical Android-to-Windows LAN acceptance remains unexecuted.
