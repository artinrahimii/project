# T0059 execution report

## Exact baseline and graph validation

- Repository/branch: `artinrahimii/project` / `main`.
- Baseline: `f8b75f66b270200c16a5ab091509b1a6750a7ada` (`V 3.0.179`).
- The ordinary contents fetch of `step three/project-graph.md` returned an empty body. The exact blob `ada660a28fe3b18dc761fdd7d8a405857cd069fa` was fetched from the tree at the baseline commit.
- The JSON parsed to 229 tasks and 426 edges; re-serialization matched the fetched JSON exactly. Content size: 1,139,025 UTF-8 bytes. SHA-1 over Git's exact `blob <size>\\0<content>` preimage matched the tree blob ID `ada660a28fe3b18dc761fdd7d8a405857cd069fa`.

## Dependency review

- `T0058 → T0059`: validated as `contract_or_interface_dependency`: the process launcher starts the local endpoint and alias consumed by `LlamaCppInferenceEngine` / `LlamaCppServerBackend`. Original `type=blocks` preserved.
- `T0056 → T0059`: validated as `sequencing_only`: the capacity evaluator is not a code/interface input to process startup. Original `type=blocks` retained as conservative ordering; readiness still gates inference.
- Outgoing edge `T0059 → T0060` and unrelated unreviewed edges remain unchanged.

## Attempt T0059-A001 — implementation

- Added `src/Host.Windows/Inference/LocalInferenceProcessLauncher.cs`. It uses `ProcessStartInfo.UseShellExecute = false` and separate `ArgumentList` items; caller input is never concatenated into a shell command.
- It validates an existing `llama-server.exe`, existing `.gguf` file, plain HTTP loopback URL, port range, and safe model alias. The server binds only to `127.0.0.1`, not a public network interface.
- Added WPF fields for executable/model paths and a **Start local engine** button. Both paths are stored as optional fields in the versioned local settings; older settings files remain compatible.
- The host owns its child process and attempts to stop the process tree on window shutdown. Process start alone does not mean the backend is healthy or the model is ready; **Connect** still checks health and model discovery.
- Added C# regression tests for argument boundaries (including a model path with spaces/metacharacters), non-loopback rejection, and invalid executable/alias rejection; wired them into the host lifecycle harness and extended the WPF startup smoke check.

## Verification limits

- .NET was unavailable for local execution in this environment, so C# compilation/tests were not run locally.
- At this implementation commit, the exact-commit GitHub Actions runs have not yet been inspected. The task therefore stays `in_progress`; no CI pass is claimed.
- A genuine `llama-server.exe` with a real GGUF model was not launched here. The test files are used only to test argument construction, not to simulate inference success. Live-model integration remains separately unverified.

## Next action

Inspect the `V 3.0.180` workflows. If the quality gate, WPF Release build/startup smoke, and host-lifecycle tests pass, record exact commit/run evidence and mark `T0059` done in a follow-up commit; otherwise record failures, fix, and rerun.


## Attempt T0059-A001 — exact-commit CI results

- Implementation commit: `9beb9234db60adad3e87da64f11b41c632f692a5` (`V 3.0.180`).
- [Quality gate run 40001969597](`https://github.com/artinrahimii/project/actions/runs/40001969597`): Python automated tests and Python syntax compilation passed. The graph-structure step failed with `ERROR GRAPH_READ: No fenced JSON graph with both 'nodes' and 'edges' was found.`
- [Product build run 40001969578](`https://github.com/artinrahimii/project/actions/runs/40001969578`): C# formatting passed, but the Windows build and host-lifecycle test job failed to compile with `CS0103: NormalizeOptionalPath does not exist in the current context`.
- These results are recorded as a failed attempt rather than hidden or counted as task completion.

## Attempt T0059-A002 — correction for rerun

- Moved `NormalizeOptionalPath` into the `LocalHostSettings` record, where `Validate` invokes it; it had mistakenly been placed inside `LocalHostSettingsStore`.
- Restored the outer `json` Markdown fence around the full graph JSON because the existing `tools/graph_guard.py` parser explicitly extracts only fenced JSON from this `.md` file. The JSON object itself was preserved and parses as 229 nodes and 426 edges.
- This correction is not marked successful until exact-commit GitHub Actions verifies it. `T0059` remains `in_progress`; no successful C# acceptance test or real-model inference is claimed.


## Attempt T0059-A002 — exact-commit CI results

- Commit `60fa66631b45eddaf3bd37a0897d502dc62bdc36` (`V 3.0.181`).
- [Quality gate run 40002334497](https://github.com/artinrahimii/project/actions/runs/40002334497): Python automated tests and syntax checks passed, but graph structure validation failed because `T0058.execution_summary.attempt_count=1` while its retained `execution_history` has two entries.
- [Product build run 40002334353](https://github.com/artinrahimii/project/actions/runs/40002334353): Windows formatting, Release build, generated Protobuf checks and WPF startup smoke passed. The portable host-lifecycle test harness failed to compile the new test source because that project did not reference the Windows WPF assembly.
- Both failures are recorded below in the task ledger; neither has been treated as a passed T0059 acceptance test.

## Attempt T0059-A003 — correction for revalidation

- Linked the actual `LocalInferenceProcessLauncher.cs` source into `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj` with a `Compile Include`, allowing the portable .NET test harness to compile and test the same process-launch implementation without referencing the WPF assembly.
- Updated the argument-construction test to inspect `ArgumentList` directly instead of reading the alternate `Arguments` property.
- Reconciled T0058's stale `execution_summary` counter with its two existing execution-history entries. Both history entries remain intact; the summary now reflects them.
- The full graph remains inside the required `json` Markdown fence so `tools/graph_guard.py` can read it. The embedded JSON parses to 229 tasks and 426 edges.
- These fixes remain pending exact-commit CI. `T0059` is still `in_progress`; no successful C# acceptance run or live-model generation is claimed.
