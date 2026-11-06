# T0202 — real Android-to-Windows LAN test

## Purpose and scope

This is an on-demand acceptance test for the production Android Libp2pInferenceRequestTransport and the production Windows .NET Libp2pPeerTransport. It proves a direct TCP/Noise session over a non-loopback private LAN interface, authenticated peer identities, request delivery, response correlation and stream closure.

The Windows process is a test fixture built from the production Windows transport. It returns a deterministic protocol response (dotnet-host-response). This test does not claim that llama.cpp inference ran or that the WPF user interface was exercised; real local inference has a separate acceptance path in T0201.

The test deliberately bypasses Rendezvous discovery so that this task isolates the data-plane LAN path. It does not change the production directory or trust policy.

## Required environment

- A Windows self-hosted GitHub Actions runner registered for this repository with labels self-hosted, windows and whitep-lan.
- Run that runner service under an elevated Windows account. The script creates a temporary inbound TCP firewall rule scoped to the selected local address, local port and LocalSubnet, then removes it in finally.
- .NET 10 SDK, JDK 21, Gradle 8.13, Android SDK (API 36 and Build Tools 36.0.0), ANDROID_HOME or ANDROID_SDK_ROOT, and adb on PATH. The workflow configures .NET, Java and Gradle; the Android SDK must already be installed on the self-hosted runner.
- Exactly one authorised physical Android device connected through ADB. It must be connected to the same private LAN as the Windows host. Wi-Fi client isolation, guest-network isolation or a VPN that blocks LAN traffic must be disabled for the test.
- A dedicated test phone. The script refuses to replace an existing com.whitep.client or com.whitep.client.test package. It removes only the app/test packages that it installed during that same invocation, after confirming they were absent at the start; it never uninstalls a pre-existing app or deletes pre-existing user data.

## Run

1. Register and start the Windows self-hosted runner with the whitep-lan label. Connect and authorise one physical Android device with USB debugging.
2. Confirm that the device and Windows host can communicate on the same LAN. Identify the Windows host's assigned RFC1918 IPv4 address.
3. In GitHub, open Actions → Android to Windows LAN acceptance → Run workflow on main.
4. Enter the Windows host's assigned private IPv4 address and an unused TCP port (default 43000).
5. Inspect the job result and download the whitep-t0202-lan-acceptance artifact. A passing run must report Android instrumentation success and the Windows fixture's final PASS marker.

The workflow is manual-only and targets the self-hosted runner because separate GitHub-hosted jobs do not share the physical LAN being tested.

## Security and evidence rules

- The Android app generates or reuses its normal Keystore-protected identity. Only its public PeerId is handed to the Windows test fixture over the connected ADB test channel.
- The Android test explicitly trusts the exact Windows PeerId supplied by the fixture for this test, sends a fixed non-sensitive protocol probe, checks the complete response correlation and revokes that test trust in finally.
- The test phone is not reset and existing app data is not removed. Use a dedicated device with the White-P app package absent before starting.
- The workflow does not publish peer identities, private addresses, prompts, responses or keys as artifacts. The report artifact contains only the source revision, test result, timestamps and test scope.
- Do not mark T0202 done until this workflow has actually passed on a real Windows host and a separate Android device over the non-loopback LAN. A code build, a loopback test or a successful workflow configuration is not LAN acceptance.
