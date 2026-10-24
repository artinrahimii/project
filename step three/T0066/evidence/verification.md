# T0066 verification evidence

- LocalInferenceBinaryVerifier.cs: SHA-256 comparison precedes the --version probe; probe is shell-free and time-bounded; file hash is rechecked after the probe.
- LocalInferenceProcessLauncher.cs: verifier runs before the managed inference-server process starts; missing expected version/hash fails closed.
- tests/test_local_inference_binary_verifier.py: source-level regression checks cover ordering, shell-free probe, timeout, recheck and launch gate.
- Exact-head CI pending; no successful test is claimed yet.
