# T0121 verification evidence — exact commit CI passed

## Baseline and graph integrity
- Repository: `artinrahimii/project`; target branch: `main`.
- Implementation commit: `969ad720db1a84c1f633031300ba26e00d48d5fa` (`V 3.0.365`).
- Graph baseline Blob SHA: `417ea870c139d3e858cb93ccfadde241feb4c6bb`; tree size 1,866,696 bytes; recomputed Git SHA-1 Blob ID matched. Full fenced JSON parsed successfully with 229 tasks and 426 edges.
- The current graph records T0121 as `in_progress`; the task is intentionally not counted as done.

## Exact-commit CI evidence
- **Host lifecycle and acceptance tests — PASS:** https://github.com/artinrahimii/project/actions/runs/40455186298/job/122026746502
  - Includes the live HTTP test for immediate announce, TTL renewal, stop and natural expiry.
  - Log output: `PASS: metadata-only discovery, periodic TTL renewal/expiry, unknown-field rejection and control-message bounds.`
- **Step Three quality guard — PASS:** https://github.com/artinrahimii/project/actions/runs/40455186284
  - Python tests, Python syntax compilation, graph-structure validation and recorded output/evidence validation.
- **Windows product build — PASS:** https://github.com/artinrahimii/project/actions/runs/40455186298/job/122026746273
  - `dotnet format whitespace`, Release build, generated Protobuf validation and WPF startup smoke.
- **Android product build — PASS:** https://github.com/artinrahimii/project/actions/runs/40455186298/job/122026746493
  - Kotlin lint, debug APK build, Protobuf round-trip and generated-source checks.
- **Real local inference — PASS:** https://github.com/artinrahimii/project/actions/runs/40455186251
  - Live llama.cpp/GGUF inference, cancellation and owned-process shutdown.
- **Aggregate product run:** https://github.com/artinrahimii/project/actions/runs/40455186298

## Remaining acceptance gap
The lease publisher is not yet wired into the WPF host participation lifecycle because the current app only binds its listener to loopback and lacks configured rendezvous/peer-reachable address settings. End-to-end host announcement/renewal lifecycle therefore remains incomplete; do not mark T0121 done.
