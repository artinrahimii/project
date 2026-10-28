# T0139 — Verification evidence

## Graph and source baseline
- Exact base: `main`, `V 3.0.485`, `1fed6eecb0a79edf0a9cec6a32c6c2bf11eaf98f`; tree `e4ceb5f8580b81ff6ad736d4d6580d85035ff8d5`.
- Graph Blob `2e7c4232c1f6c756ad39ebdfaf4f29c9a4cafef8`; original bytes **2,351,270 UTF-8 bytes**, string length **2,229,438 characters**. Git Blob SHA-1 over `blob {length}\0` plus complete raw bytes matched the tree entry. Full JSON parsed as 229 tasks / 417 active edges.
- Current task status remains `in_progress`; A001 failure is preserved in graph history and A002 is a correction attempt, not yet a pass.

## A001 — failed exact-head checks (V 3.0.485)
- [Quality job 123096849080](https://github.com/artinrahimii/project/actions/runs/40767883598/job/123096849080): 199 tests ran; three existing source-contract tests failed because MainActivity had removed required explicit P2P preview/no-delivery copy.
- [Android job 123096849669](https://github.com/artinrahimii/project/actions/runs/40767883644/job/123096849669): ktlintCheck failed on uppercase Compose function names, multi-line expression formatting, one line >120 chars and a missing trailing enum comma. APK and Android JUnit tasks were skipped.
- [HostLifecycle job 123096849547](https://github.com/artinrahimii/project/actions/runs/40767883644/job/123096849547): passed. This does not imply Android build success.

## A002 — corrective changes
- Restores the required old preview and not-sent phrases without claiming transport has been implemented.
- Renames private Compose functions to lowercase camel case, shortens privacy wording, uses ktlint-compliant continuation formatting, and adds the enum trailing comma.
- Updates the new source-contract check to assert the privacy sentence's components without requiring a single literal line.
- A002 CI status: **pending**; no passing result is yet claimed.
