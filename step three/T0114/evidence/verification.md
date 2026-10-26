# T0114 verification evidence — pending exact-head CI

## Baseline and reviewed edges
- Repository: `artinrahimii/project`, branch `main`, base `6c246d0485c36f970c5fcd1ae7a0881b3a98f091`.
- Graph Blob `58873fe76615a7c91ac57aa6d74ff91e53f283d7` parsed to 229 tasks / 426 edges.
- `T0113 → T0114`: reviewed as `sequencing_only`; shared listener file but no new task-specific interface is consumed. `blocks` retained.
- `T0107 → T0114`: reviewed as `sequencing_only`; request coordinator output does not drive bind-address or WPF endpoint status. `blocks` retained.

## Implementation
- `src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs`: fails closed unless `IPAddress.IsLoopback(bindAddress)` is true.
- `tests/HostLifecycle.Tests/HostNetworkListenerParticipationTests.cs`: rejects IPv4 wildcard, IPv6 wildcard and non-loopback bind addresses.
- `tests/test_network_listener_exposure.py`: checks the constructor guard and WPF endpoint/port/participation status display.
- `docs/decisions/ADR-0003-tcp-request-framing.md`: records enforced loopback-only binding and the UI status behavior.

## Verification
- V 3.0.345 Python tests and syntax checks passed. The graph guard failed on a T0113 `FAILED_ATTEMPT_COUNT_MISMATCH` only; graph metadata correction is in V 3.0.346.
- Exact-head HostLifecycle/Windows WPF build and startup smoke: pending.
- Do not mark T0114 done until these jobs pass.
