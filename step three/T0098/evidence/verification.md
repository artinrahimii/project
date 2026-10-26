# T0098 verification evidence

- **Repository / ref:** `artinrahimii/project`, branch `main`, base `7d39ecef9ab3cd273dbdddcfce8104a0d19c5308`.
- **Graph source:** `step three/project-graph.md`, Blob `493077b2a63338ab465570488f48f9c909484a89`.
- **Integrity:** the Git tree reports 1,585,789 bytes. The fetched original Markdown, including the opening and closing JSON code fences, is the same byte length; recomputed Git Blob SHA-1 matches `493077b2a63338ab465570488f48f9c909484a89`. The enclosed JSON parses completely and has 229 unique nodes and 426 edges. The ordinary file-content endpoint returned an empty body for this large file, so the exact-commit Git Blob was used.
- **Incoming edge:** `T0097 -> T0098` remains a validated sequencing-only edge. The T0097 capability gate has a `done` state in the validated graph and prior exact-commit CI evidence in `step three/T0097/evidence/verification.md`.
- **Outgoing edge review:** `T0098 -> T0099` changed from `unreviewed` to `hard_dependency` with rationale/evidence. T0099's stated Busy-rejection behavior is layered onto the same coordinator created here.
- **Implementation:** `src/Host.Windows/Requests/HostRequestCoordinator.cs` uses a capacity-one `SemaphoreSlim`; `finally` releases the gate on success, cancellation, or exception.
- **Automated acceptance source:** `tests/HostLifecycle.Tests/HostRequestCoordinatorTests.cs` checks that two operations do not overlap and the semaphore is reusable after a failure. The existing CI harness invocation in `.github/workflows/product-build.yml` covers this test after it is registered in `Program.cs`.
- **Execution status:** code and test source are committed by the implementation attempt; the local runner has no .NET SDK, so C# tests were not run locally. Exact-commit GitHub Actions remains the completion gate.


## CI failure and correction

- Implementation commit tested: `451980f2ec009f7915c10fa705a53789a6ea33b9` (`V 3.0.306`).
- Step Three quality gate: passed, run [40306971116](https://github.com/artinrahimii/project/actions/runs/40306971116).
- HostLifecycle job: failed during project evaluation with `NETSDK1022`, because `HostRequestCoordinatorTests.cs` was explicitly added even though SDK default Compile items already included it; the C# tests themselves had not started running.
- Fix: removed only the duplicate test-file Compile element from `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`; kept the explicit linked compile for the source under `src/Host.Windows`.
- Validation after fix: pending a new commit's GitHub Actions. T0098 remains `in_progress` until it succeeds.
