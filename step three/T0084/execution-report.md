# T0084 — Execution report

## Revision and graph integrity

- Target branch: `main`; implementation and exact-head verification commit: `da9c9bd5fd04e09a9307b8617bee74a83c592956` (`V 3.0.254`).
- Graph was read from that commit's recursive Git tree at `step three/project-graph.md`, blob `6a700132d194230953f3413ad97fcc32dc7bc829`.
- Downloaded the blob through the Git Blob API. UTF-8 size: 1,383,730 bytes; the embedded fenced JSON parsed successfully. Recomputed the Git SHA-1 using `blob 1383730\0 + content`: it matched `6a700132d194230953f3413ad97fcc32dc7bc829`. The JSON round-trip preserved the original formatting before this task's targeted ledger edits.
- Pre-update graph: 229 tasks, 426 edges, no duplicate task IDs, and no dangling edge endpoints.

## Implementation and observed behavior

- Added an explicit confirmation before disconnecting the active model. The handler rejects removal while the host reports `Busy`, clears the active catalog selection and persisted active catalog ID, and calls `HostInferenceSession.DisconnectAsync`.
- `DisconnectAsync` serializes with inference/reconnect operations using the shared operation gate, transitions the session to `Stopped`, disposes an `IDisposable` backend, and replaces it with an unconfigured backend. The app also stops the local engine process it owns.
- The GGUF model file is intentionally retained on disk. Disposing a client and stopping an app-owned process cannot guarantee memory release from an externally managed llama.cpp server, which has no portable unload operation.

## Exact-head acceptance evidence

All cited workflows ran against the exact implementation commit `da9c9bd5fd04e09a9307b8617bee74a83c592956`:

- Step Three quality gate **passed**: https://github.com/artinrahimii/project/actions/runs/40170781358
- Host lifecycle unit/integration tests **passed**. The live log includes: `PASS: disconnect disposes backend resources and leaves the host stopped.` https://github.com/artinrahimii/project/actions/runs/40170781315/jobs/121100991241
- Windows WPF Release build, formatting verification and startup smoke test **passed**: https://github.com/artinrahimii/project/actions/runs/40170781315/jobs/121100990903
- Android debug APK build, ktlint and Protobuf round-trip tests **passed**: https://github.com/artinrahimii/project/actions/runs/40170781315/jobs/121100991035
- Real local llama.cpp/GGUF inference integration **passed**: https://github.com/artinrahimii/project/actions/runs/40170781304

The earlier implementation attempt recorded that tests had not yet run; this follow-up preserves that history and adds the later exact-head CI result rather than overwriting the earlier attempt.

## Dependency review

- `T0083 -> T0084`: validated as `contract_or_interface_dependency`; disconnect shares the operation gate used by generation and reconnect.
- `T0075 -> T0084`: validated as `sequencing_only`; the removal flow resets catalog selection/settings but does not directly call `OfflineModelCatalogStore`. The conservative `type=blocks` and execution order remain unchanged.
- `T0084 -> T0085`: validated as `contract_or_interface_dependency`; updating an active model must use a safe disconnect before replacement. The existing installer refuses to overwrite an existing destination, so atomic update and rollback remain work for T0085.

## Outcome

**T0084 is complete** against its current acceptance criteria: real UI/session changes exist, the dedicated disposal regression is registered and passed in CI, and the WPF/Android/quality/live-inference workflows passed at the exact implementation commit. T0085 remains not started and is the next task to review and execute. No claim is made that the model file is deleted or that an external server's RAM is forcibly released.
