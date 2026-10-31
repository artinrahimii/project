# T0120 verification evidence — accepted on exact V 3.0.363

## Baseline integrity
- Repository: `artinrahimii/project`, branch `main`.
- Tested implementation commit: `63cbad84a6b34367c3ec306116d2b0d8d8320d81` (`V 3.0.363`).
- Graph source at the tested commit was retrieved via Git Blob API: `76d45fbc7da484abdea64cc7b15a1f09aa8b060a`; Git Blob SHA-1 matched the complete original source (1,862,684 UTF-8 bytes, 1,740,976 returned text characters) and the fenced JSON reconstructed exactly to 229 tasks and 426 edges.

## Exact-head CI — all passed
- **HostLifecycle / PeerAnnouncementClientAcceptanceTests:** PASS — https://github.com/artinrahimii/project/actions/runs/40452476275/job/122018153179
  - Live loopback announcement and capability discovery.
  - Re-announcement replaces current metadata.
  - Unsupported capability yields no peer.
  - No central inference route exists.
  - Unknown `prompt` field rejected.
  - Raw body above 16 KiB rejected.
  - Non-loopback HTTP endpoint configuration rejected.
- **Windows WPF product build and startup smoke:** PASS — https://github.com/artinrahimii/project/actions/runs/40452476275/job/122018153275
- **Android APK/Protobuf/lint and generated source checks:** PASS — https://github.com/artinrahimii/project/actions/runs/40452476275/job/122018153379
- **Real llama.cpp/GGUF inference integration:** PASS — https://github.com/artinrahimii/project/actions/runs/40452476398
- **Step Three quality gate:** PASS — https://github.com/artinrahimii/project/actions/runs/40452476336

## Earlier failed attempts kept
- V 3.0.361 / A001: ambiguous newline test input failed before loopback tests.
- V 3.0.362 / A002: validation rejected the explicit newline, but the test helper mistakenly treated the expected exception as a failure.
- V 3.0.363 / A003: corrected helper; all exact-head checks listed above passed. The graph retains the failures and increments the accepted result.

## Privacy and scope
The control plane handles peer ID, direct addresses, capability metadata and expiry only. Unknown JSON fields are disallowed, announcement history is not retained, and raw messages are bounded. This acceptance does not prove Android-to-Windows end-to-end inference routing; `AC-P2P-01` remains **NOT RUN**.
