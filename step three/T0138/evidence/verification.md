# T0138 — Verification evidence

## Exact graph source integrity
- Branch: `main`; source commit: `V 3.0.483` — `18060380d0cdfb4a8231e72ac6a5a469fce3fddd`; tree: `eda5823476f130d81233009577e650cdaecc832b`.
- Graph Blob: `6d52a006f0a9f57e934e38ee96efe55060176b05`; **2,342,131 UTF-8 bytes**, **2,220,306 characters**.
- Git Blob ID recalculated over the complete original bytes including Markdown fences: `6d52a006f0a9f57e934e38ee96efe55060176b05`; matches the tree Blob entry. Complete JSON parsed to **229 tasks / 417 active edges**.

## T0138-A001 — failed attempt preserved
- [Quality run V 3.0.482](https://github.com/artinrahimii/project/actions/runs/40752311175): 195 tests ran and one subtest failed due to the test/ADR phrase mismatch. No successful result was inferred from that run.

## T0138-A002 — corrected and verified
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/40752968011): all 195 Python tests passed; syntax and evidence/graph checks passed. The graph guard emitted 0 errors, 1 warning for T0001's existing unmapped free-text prerequisite, retained as metadata.
- [HostLifecycle](https://github.com/artinrahimii/project/actions/runs/40752968080/job/123044253346) passed.
- [Windows WPF](https://github.com/artinrahimii/project/actions/runs/40752968080/job/123044253238) passed build, generated Protobuf, UI smoke and host identity/trust checks.
- [Android APK](https://github.com/artinrahimii/project/actions/runs/40752968080/job/123044252712) passed lint, package, Protobuf and generated-source checks.

## Privacy boundary
- The receiving host can inspect `InferenceRequest.prompt` and `context_capsule` after transport decryption. Channel encryption is not end-to-end encryption against that endpoint.
- This disclosure does not claim Android-to-Windows live routing or no-storage runtime proof. `T0128`, `AC-P2P-01`, and `AC-PRIV-01` remain NOT RUN.
