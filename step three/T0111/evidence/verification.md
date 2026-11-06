# T0111 verification evidence

## Baseline integrity
- Repository/branch: `artinrahimii/project` / `main`.
- Tested implementation commit: `2b11ab4f697760556c1a4f0d4cc4542f89e14961` (`V 3.0.340`).
- Baseline commit: `50e2d0605b1c0c41090f39c0673c0bebe373b382`.
- The graph at the baseline was read by Git Blob API because ordinary file fetch returned empty content. The exact original graph was 1,737,836 UTF-8 bytes. The Git object SHA-1 recomputed over the full original bytes matched Blob `0fc67b4190acfd21bbda9fe4ffb208c5a6b0f757`; fenced JSON parsed as 229 tasks and 426 edges.
- The online main ref and commit object were verified against `2b11ab4f697760556c1a4f0d4cc4542f89e14961` before CI results were recorded.

## Acceptance and exact-head CI
- Step Three quality gate: PASS — https://github.com/artinrahimii/project/actions/runs/40414166283
- HostLifecycle tests: PASS — https://github.com/artinrahimii/project/actions/runs/40414166219/job/121892143191
- Windows WPF Release build, formatter, generated C# Protobuf validation, startup and agent-tool policy/capability checks: PASS — https://github.com/artinrahimii/project/actions/runs/40414166219/job/121892143042
- Android lint, APK build, Protobuf round-trip tests, generated Java/Kotlin sources and artifact upload: PASS — https://github.com/artinrahimii/project/actions/runs/40414166219/job/121892143197
- Live local llama.cpp + pinned GGUF inference acceptance: PASS — https://github.com/artinrahimii/project/actions/runs/40414166184
- Full product workflow: PASS — https://github.com/artinrahimii/project/actions/runs/40414166219
- All runs above were triggered by and verified for the exact implementation commit `2b11ab4f697760556c1a4f0d4cc4542f89e14961`.

## Implementation evidence
- `protocol/whitep/v1/messages.proto`: field 8 adds the request protocol version without changing fields 1–7.
- `src/Shared.Contracts/ProtocolMessageValidator.cs`: rejects unsupported or malformed versions, identifiers outside the documented bounded ASCII alphabet, invalid deadlines and expired deadlines using the existing typed REQUEST_EXPIRED wire code.
- `src/Host.Infrastructure/ProtocolMessageParser.cs` and `src/Host.Application/HostInferenceSession.cs`: pass protocol version into shared validation.
- `src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs`: rechecks current local participation before invoking the request callback.
- C# tests cover bad versions, unsafe request IDs, deadline expiry and the participation-revocation race. Kotlin fixtures populate the new field; static contract tests check the schema and validation.
- ADR-0003 documents the one-frame TCP contract and clarifies that local participation is not cryptographic peer authentication.

## Dependency review
- `T0110 → T0111`: `contract_or_interface_dependency`, validated with parser/frame reader and acceptance-test evidence.
- `T0107 → T0111`: `sequencing_only`, validated because T0111 consumes no T0107-specific API/artifact. The existing `blocks` edge was retained.
- `T0111 → T0112` remains `unreviewed`; it must be reviewed before changing work that depends on it.
