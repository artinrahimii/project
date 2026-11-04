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

## T0202-A004 — successful automated CI (not LAN acceptance)

- [Step Three quality gate, run 41900885151](https://github.com/artinrahimii/project/actions/runs/41900885151) — success.
- [White-P product build, run 41900885262](https://github.com/artinrahimii/project/actions/runs/41900885262) — success.
- [Rendezvous container, run 41900885166](https://github.com/artinrahimii/project/actions/runs/41900885166) — success.
- Windows WPF Release build, Protobuf C# generation, startup smoke, agent-tool policy/capability/process tests, DPAPI identity/peer trust tests — passed.
- HostLifecycle unit/integration tests — passed.
- Android Windows .NET peer fixture build, KtLint, debug APK build, merged permissions check, JVM unit tests, Keystore emulator instrumentation, generated Java/Kotlin Protobuf check and APK upload — passed.
- Android APK, Windows host and Android instrumentation/unit-test report artifacts were published.

**Scope limitation:** The successful Android Keystore emulator instrumentation is not an Android-to-Windows LAN test. The manual workflow needs one physical Android handset and the Windows host on the same non-loopback private LAN. That workflow was not executed, so T0202 remains blocked.

## T0202-A005 — Android CI follow-up (not LAN acceptance)

- Source commit: `fff8b813cc90bd853b83b01b87a42fc57e58657c` (`V 3.0.930`).
- [Product Build run 41902912019](https://github.com/artinrahimii/project/actions/runs/41902912019): Windows WPF and HostLifecycle jobs passed.
- First Android job attempt passed .NET fixture build, KtLint, APK build and permission checks, then failed 17 Robolectric test class setups (17 of 131) at `MavenArtifactFetcher.java:130` with `HttpURLConnection` I/O errors.
- The failed Android job was retried as run attempt 2. Confirmed successful steps at snapshot: fixture build, KtLint, debug APK build, permissions policy, `testDebugUnitTest`, KVM setup and temporary fixture cleanup.
- Android Keystore emulator instrumentation was still running at snapshot time. No result is inferred for that pending step.
- This rerun is supporting CI evidence only. The manual physical-device Android-to-Windows LAN workflow was not executed; T0202 stays blocked.

## T0202-A005 — final retry result

- Product Build run [41902912019](https://github.com/artinrahimii/project/actions/runs/41902912019), run attempt 2: overall success.
- Windows WPF and HostLifecycle jobs: success.
- Android fixture build, Kotlin lint, APK build, merged permission policy, JVM unit tests, Keystore emulator instrumentation, Protobuf Java/Kotlin generation verification and artifact uploads: success.
- `connectedDebugAndroidTest` finished 12 tests. `AndroidWindowsLanIntegrationTest.connectsToWindowsHostOverNonLoopbackLan` was **SKIPPED** on the ordinary emulator job because the manual LAN host arguments were absent. This is not evidence of a successful network connection.
- Uploaded artifacts: APK ID 13358249815; JVM report ID 13359797798; Keystore instrumentation report ID 13360107291.
- The first Android attempt's 17 Robolectric MavenArtifactFetcher I/O setup failures did not recur in the retry; `testDebugUnitTest` passed on attempt 2.

**T0202 remains blocked:** `.github/workflows/android-windows-lan-acceptance.yml` has not been run against a physical Android device and an independent Windows host on the same non-loopback LAN.
