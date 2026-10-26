# T0114 execution report — in progress, awaiting exact-head CI

- **Task:** restrict unnecessary public network access and show port/connection status to the user.
- **Attempt:** `T0114-A001`.
- **Baseline:** `main` at `6c246d0485c36f970c5fcd1ae7a0881b3a98f091` (`V 3.0.344`); graph Blob `58873fe76615a7c91ac57aa6d74ff91e53f283d7` parsed completely to 229 tasks and 426 edges, with Blob SHA-1 matched.
- **Existing behavior verified:** `MainWindow.xaml.cs` already constructs the lifecycle with `IPAddress.Loopback`, shows `endpoint.Address:endpoint.Port` only when participation is active, and clears the UI status when disabled. The reusable listener constructor itself, however, accepted wildcard addresses from a different caller.
- **Implementation:** `ParticipationGatedNetworkListener` now rejects bind addresses that are not loopback before a socket can be started. Acceptance tests cover IPv4 wildcard, IPv6 wildcard and a non-loopback address. A Python regression guard checks the enforced bind policy and the actual endpoint/status UI strings. ADR-0003 records the enforced boundary.
- **Dependency review:** `T0113 → T0114` and `T0107 → T0114` reviewed as `sequencing_only`; each retains its `blocks` edge with evidence and rationale. No unreviewed incoming edge was changed silently.
- **Tests:** exact-head CI pending; task remains `in_progress`.