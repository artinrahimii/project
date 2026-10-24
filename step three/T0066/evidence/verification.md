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
