# T0110 verification evidence — accepted on V338

## Graph/source integrity
- Repository `artinrahimii/project`, branch `main`; implementation/test commit `86267255efa01fe899a26d58f5e5beec4f860c75` (`V 3.0.338`).
- The V338 pre-final graph Blob was `2bfd0becbf7f7f813f0a4efb4a1ec2f339e2c8ed`: 1,732,522 UTF-8 bytes / 1,610,882 Unicode characters. Recomputed Git Blob SHA-1 matched; complete fenced JSON parsed to 229 tasks, 426 edges, and 22 sections.
- Baseline V336 graph Blob `355f2972c2e9ad3acd4c022644440d55feb17748`: 1,718,063 UTF-8 bytes / 1,596,320 characters, SHA matched and JSON parsed completely.

## Exact-head gates — all PASS on V338
- HostLifecycle, including T0108 participation, T0109 lifecycle, and T0110 real TCP framing acceptance: [PASS](https://github.com/artinrahimii/project/actions/runs/40406054183/job/121863964765).
- Step Three quality gate: [PASS](https://github.com/artinrahimii/project/actions/runs/40406054261).
- Windows WPF Release build and startup smoke, Android lint/APK/Protobuf/generated-source checks and artifact upload: [PASS](https://github.com/artinrahimii/project/actions/runs/40406054183).
- Real local llama.cpp/GGUF inference: [PASS](https://github.com/artinrahimii/project/actions/runs/40406054061).

## Contract and safety boundary
- The shared parser `ProtocolMessageParser.ParseInferenceRequest` validates the typed Protobuf after the frame reader bounds raw bytes to 16,384.
- One frame per TCP connection; 4-byte unsigned big-endian length prefix; invalid, oversized and incomplete input is closed without payload logging; partial-frame reads time out and are cancelled during shutdown.
- A valid typed request reaches an optional callback only. No callback is wired to inference, no peer response is sent, and full P2P/encryption/Android interoperability are not claimed.
