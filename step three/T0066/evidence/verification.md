# T0066 verification evidence

- LocalInferenceBinaryVerifier.cs: SHA-256 comparison precedes the --version probe; probe is shell-free and time-bounded; file hash is rechecked after the probe.
- LocalInferenceProcessLauncher.cs: verifier runs before the managed inference-server process starts; missing expected version/hash fails closed.
- tests/test_local_inference_binary_verifier.py: source-level regression checks cover ordering, shell-free probe, timeout, recheck and launch gate.
- Exact-head CI pending; no successful test is claimed yet.

## Exact-head failure and repair

- Exact inspected head `08583bd892e3a2e5fdf081fd2e80e44afdecbab0`: quality gate passed, but Windows host compilation failed on missing `System.IO` names; the portable lifecycle test project omitted the verifier source. Real inference stopped at compilation and did not exercise verification.
- Repair added explicit `System.IO`, linked `LocalInferenceBinaryVerifier.cs` into the test project, and configured the live workflow's expected build version and binary SHA-256 after the pinned archive hash check.
- New exact-head CI verification is pending; no successful binary-validation claim is made yet.

## Regression-test import failure and correction

- Quality run 40104554822 failed on a Python syntax error in the newly added `HOST_TEST_PROJECT` path; this was not a verifier runtime result.
- The missing quote is corrected in the next commit. Exact-head validation is pending.

## T0066-A006 — verified acceptance evidence

- Exact-head quality gate: [run 40104682026](https://github.com/artinrahimii/project/actions/runs/40104682026): 123 Python tests passed; syntax, graph, output and evidence checks passed.
- Windows host build, generated Protobuf validation, WPF startup smoke, host lifecycle tests and Android APK checks: [product run 40104681998](https://github.com/artinrahimii/project/actions/runs/40104681998), all jobs passed.
- Full product build on implementation commit: [run 40104554847](https://github.com/artinrahimii/project/actions/runs/40104554847), all three jobs passed.
- Real local inference on implementation commit: [run 40104554823](https://github.com/artinrahimii/project/actions/runs/40104554823). The pinned b11443 archive hash matched; executable build `11443` and SHA-256 `4cdaae426a798b49717e3ce9b8a7756a16355c83c812d9d59bc99e26ae6d3968` were validated; the pinned GGUF model generated a non-empty response and active-generation cancellation stopped the owned process cleanly.
- The T0066 criterion is satisfied. Historical failures remain recorded; this evidence does not prove overall project completion.
