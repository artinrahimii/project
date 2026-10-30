
# T0162 verification evidence — V 3.0.610 audit and next correction

- **Baseline:** `main` at `55905b330d178885a13fdf0bc46981960f38e97f` (`V 3.0.610`).
- **Step Three quality gate — PASS:** https://github.com/artinrahimii/project/actions/runs/41040016565.
- **HostLifecycle — PASS:** https://github.com/artinrahimii/project/actions/runs/41040016495 (job 124010228954).
- **Windows WPF — partial evidence only:** C# formatting, Release build, generated Protobuf validation and startup smoke passed; process/policy/DPAPI checks were still running at inspection. No overall Windows conclusion is recorded.
- **Android KtLint — FAIL:** https://github.com/artinrahimii/project/actions/runs/41040016495 (job 124010229140). The `supportsModel` chain was fixed, but three further chained-access style violations remain in prompt/capsule byte-size calculations at `HostCandidateSelector.kt:106`; Android APK/JVM/Protobuf were skipped.
- **A003 correction:** use named locals for prompt text, UTF-8 bytes, capsule bytes, and serialized request length so the Kotlin linter no longer encounters multiple property/method accesses chained on one line.
- **Task state:** still `in_progress`; no Android behavior tests have passed yet for T0162.
