# T0120 verification evidence — first runtime attempt failed; corrected test pending

## Baseline integrity
- Repository: `artinrahimii/project`, `main`.
- First implementation commit: `V 3.0.361`, `9985bb108c02226f1817b6d6cd2562467703e487`.
- The graph Blob was fetched in full and reconstructed exactly before the task update; it contains 229 tasks and 426 edges.

## Observed exact-head results
- **Step Three quality gate — PASS:** https://github.com/artinrahimii/project/actions/runs/40451885488. This validates Python tests, graph structure and recorded evidence, not the C# service runtime.
- **HostLifecycle — FAIL:** https://github.com/artinrahimii/project/actions/runs/40451885440. Existing host/P2P tests passed, but `VerifyContractSurfaceIsMetadataOnly` failed because the capability control-character test input did not unambiguously produce a newline. The loopback HTTP assertions had not executed.

## Correction
- `PeerAnnouncementClientAcceptanceTests.cs` now builds the newline explicitly using `string.Concat("inference", (char)10, "request")`.
- `T0120-A001` remains recorded as failed; `T0120-A002` is pending exact-head CI. No runtime acceptance claim is made yet.

## Runtime acceptance still required
- Announce / capability discovery / replacement over a real loopback HTTP service.
- Unsupported capability returns no peers.
- No inference route exists.
- Extra prompt field is rejected before directory storage.
- Raw request body over 16 KiB is rejected.
- Windows WPF build, Android APK checks, and graph/evidence quality gate pass on the same commit.
