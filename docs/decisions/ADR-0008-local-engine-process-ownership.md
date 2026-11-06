# ADR-0008: Verified local engine process execution and ownership

- **Status:** Accepted for the managed Windows-host `llama-server.exe` path implemented in the current repository.
- **Date:** 2026-10-27
- **Related task:** T0007 (trace technical decisions to production source, configuration and executable tests); T0059/T0063/T0066/T0067 (managed local engine, process ownership and binary verification).
- **Related requirement:** REQ-004 in `docs/requirements.md` (real local inference backend).

## Decision

1. The host must fail closed if `WHITEP_LLAMA_SERVER_VERSION` or `WHITEP_LLAMA_SERVER_SHA256` is absent. The expected version and hash must come from a trusted release selection; a supplied hash only proves equality to that configured value, not independent publisher authenticity.
2. Before launching the managed server, `LocalInferenceBinaryVerifier` verifies that the selected file is an existing `llama-server.exe`, checks its SHA-256 using a fixed-time comparison before running `--version`, invokes that probe without a shell and with a bounded timeout, requires a successful exit and the expected version text, then hashes the file again to detect replacement during verification.
3. `LocalInferenceProcessLauncher.CreateStartInfo` accepts only an existing `.gguf` model file, an allow-listed model alias, and a plain-HTTP loopback URI on port 1024–65535. It disables shell execution and passes every option/value through separate `ProcessStartInfo.ArgumentList` entries. The managed server binds to `127.0.0.1` and receives `--no-webui`; it must not be exposed as a public network listener by this path.
4. The launcher owns only the process it starts. `Stop` and `Dispose` detach the owned process, request termination of its process tree, wait up to five seconds, and dispose the process handle. It does not terminate an externally launched llama.cpp process.
5. Tests must distinguish source/argument-construction contracts from live-model acceptance. Fixture files used to inspect argv are not executable models. The opt-in real-inference workflow is the evidence path for a verified local binary and a real model response; this ADR alone does not pass a release acceptance gate.

## Implementation and executable evidence

- Process validation, shell-free argument construction, loopback binding and process-tree cleanup: `src/Host.Windows/Inference/LocalInferenceProcessLauncher.cs`.
- SHA-256/version preflight, bounded `--version` probe and post-probe rehash: `src/Host.Windows/Inference/LocalInferenceBinaryVerifier.cs`.
- Executable argument-construction regressions, including a model path containing spaces and shell metacharacters, non-loopback rejection and unsafe alias rejection: `tests/HostLifecycle.Tests/LocalInferenceProcessLauncherTests.cs`.
- Harness registration and compilation of the source under test: `tests/HostLifecycle.Tests/Program.cs` and `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`.
- Source-contract regression for verifier/launcher ordering and CI configuration: `tests/test_local_inference_binary_verifier.py`.
- The opt-in local-model execution path and release/hash inputs: `.github/workflows/real-inference.yml`.
- Decision traceability guard and register: `tests/test_decision_traceability.py` and `docs/decisions/README.md`.

## Evidence boundary

The source and test files are linked here so changes to launch ordering, shell handling, loopback restrictions, version/hash enforcement or process ownership can be caught by the repository's executable/source-contract suites. Passing these tests does not establish a generalized process sandbox, independently attest the trustworthiness of caller-supplied expected hashes, or complete Android-to-Windows P2P request routing. `AC-P2P-01` remains **NOT RUN** until separate end-to-end evidence is recorded.
