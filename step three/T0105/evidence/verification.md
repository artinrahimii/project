# T0105 verification evidence

## Exact source snapshot

- Repository/branch: `artinrahimii/project`, `main`.
- Base commit: `165456c176a1f90675e080f7652c010255477e3d` (`V 3.0.322`); tree SHA: `66c539860c1b8c93cfb5400bc68c953dd1efeade`.
- Graph path: `step three/project-graph.md`; expected Blob SHA: `1dc49ef40acd0c64b49928c641d8474ec7efb16f`.
- The ordinary file fetch returned empty text despite a matching reported Blob SHA. The fallback Git Blob fetch was used; the complete UTF-8 content measured **1,671,793 bytes**, matching the tree entry, parsed as the full fenced JSON, and its calculated Git Blob SHA matched `1dc49ef40acd0c64b49928c641d8474ec7efb16f`.
- The validated graph contains 229 unique task nodes and 426 edges. Before this attempt, T0105 was `not_started`; T0104 was `done` with exact-head CI evidence and T0097 was `done`.

## Reviewed edge

- `T0104 -> T0105`: `type=blocks` preserved; classification changed from `unreviewed` to `sequencing_only`; validation changed to `validated`.
- Evidence inspected: `src/Host.Windows/Requests/HostRequestCoordinator.cs`, `tests/HostLifecycle.Tests/HostRequestCoordinatorTests.cs`, `src/Host.Windows/Requests/HostRequestResponseDeliveryGate.cs`, `tests/HostLifecycle.Tests/HostRequestResponseDeliveryTests.cs`, and T0104's execution/evidence reports.
- Rationale: the response-callback guard and temporary active-request cleanup do not exchange a required API/output. The edge remains only as conservative sequencing.

## Test added

- `tests/HostLifecycle.Tests/HostRequestCoordinatorTests.cs`: controlled operation failure with assertions for exception propagation, clearing of the active cancellation reference, release of the single-request semaphore, and acceptance of the next request.
- The test project already compiles the production `HostRequestCoordinator.cs`; `Program.cs` already calls `HostRequestCoordinatorTests.RunAsync()`.
- Local execution was not available because the .NET SDK is missing. Exact-commit CI has not yet run at the time of this record.
