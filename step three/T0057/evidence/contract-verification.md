# T0057 contract evidence

## Exact source snapshot

- Repository: `artinrahimii/project`
- Branch: `main`
- Base commit: `9b91b5b35396dab4a6106fb84f1cf33f2a1784d5` (`V 3.0.174`)
- Graph path: `step three/project-graph.md`
- Graph Blob ID from the tree/contents API: `351950132da3fa8e4f36b703c16c64cad004ec84`
- The complete returned blob content was parsed as JSON: 229 task nodes, 426 edges, no duplicate node IDs, and no unresolved task-ID dependencies. The content-derived Git blob SHA matched the advertised Blob ID.
- Graph source was reserialized identically before the targeted edits; only the T0057 node, the two reviewed edges and append-only edge-change history, and `updated_at` were changed in the JSON payload.

## Contract delivered

- `src/Host.Application/Inference/IInferenceEngine.cs`
- `tests/HostLifecycle.Tests/InferenceEngineContractTests.cs`
- `tests/HostLifecycle.Tests/Program.cs`

The regression test verifies the public interface signatures for discovery, model loading, generation with a CancellationToken, explicit generation cancellation, model unloading, asynchronous disposal, and discovery-result fields. This is a compile-time/reflection contract check, not a live backend or model test.

## Dependency evidence

- T0056 capacity/lifecycle implementation: `src/Host.Windows/MainWindow.xaml.cs`, `src/Host.Domain/HostCapabilities.cs`, `src/Host.Application/HostInferenceSession.cs`.
- Existing application abstraction: `src/Host.Application/IInferenceBackend.cs`.
- Downstream adapter task statement: `step one/V1.0.md` (T0057 interface and T0058 LlamaCppInferenceEngine implementation).
- The `type: blocks` values on both edges are retained. Classification rationale is recorded in `edge_change_log` in the graph.

## Test status

- Local .NET harness: **NOT RUN** in this session.
- Exact-commit CI: **PENDING** at this point.
- Live llama.cpp/model acceptance: **NOT PART OF T0057** and remains unverified.
