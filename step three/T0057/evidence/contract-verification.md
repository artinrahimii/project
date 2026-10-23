# T0057 contract evidence

## Exact source snapshot and graph integrity

- Repository: `artinrahimii/project`
- Branch: `main`
- Base implementation commit: `6046193446183ba1298079e223b66f8813c89e00` (`V 3.0.175`).
- Graph path: `step three/project-graph.md`; complete content was obtained via Git Blob API after the ordinary file fetch returned an empty string.
- Verified graph Blob ID: `351950132da3fa8e4f36b703c16c64cad004ec84`. The byte-derived Git blob hash matched; complete JSON parsed to 229 nodes and 426 edges with no duplicate task IDs or unresolved task-ID dependencies.

## Contract delivered

- `src/Host.Application/Inference/IInferenceEngine.cs`
- `tests/HostLifecycle.Tests/InferenceEngineContractTests.cs`
- `tests/HostLifecycle.Tests/Program.cs`

The regression test covers discovery, model loading, cancellable generation, explicit cancellation, unloading, disposal, and discovery-result fields. It is a reflection contract test, not a live inference test.

## A001 failure and A002 correction

- A001 exact-commit host CI failed: [job log](`https://github.com/artinrahimii/project/actions/runs/39967342302/job/120460555222`).
- The compiler error was `CS0103`: `Require` is a local function in top-level `Program.cs` and is not available to the separate `InferenceEngineContractTests` class.
- A002 replaces that use with an inline guard which throws if the interface lacks `IAsyncDisposable`.
- A002 CI results are **PENDING** at time of this record. Do not mark T0057 done until the host harness and required product checks pass on the corrective commit.

## Dependency evidence

- T0056 capacity/lifecycle: `src/Host.Windows/MainWindow.xaml.cs`, `src/Host.Domain/HostCapabilities.cs`, `src/Host.Application/HostInferenceSession.cs`.
- Existing application abstraction: `src/Host.Application/IInferenceBackend.cs`.
- Downstream task definitions: `step one/V1.0.md` (T0057 and T0058).
- The original `type: blocks` remains on both reviewed edges; classifications and rationales are recorded in the graph's append-only edge-change log.

## Scope limit

Live llama.cpp/model acceptance and full inference are outside T0057 and remain unverified.
