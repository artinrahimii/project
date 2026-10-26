# T0097 verification evidence

- Target: `artinrahimii/project`, branch `main`; base commit `4546363bedbc758d514fddbb0d933e4c89880d5d` (`V 3.0.303`).
- Graph Blob: `4d5b78bccec56c93f0d9d07c3823d91ae46214e1`; full response measured 1,562,437 UTF-8 bytes / 1,440,408 Unicode characters. The JSON parsed fully, and Git Blob SHA-1 over the original bytes matched the tree Blob ID. Graph structure: 229 unique task IDs and 426 edges.
- `IInferenceBackend` exposes health and text generation only. `LlamaCppServerBackend` sends one user-text message to `v1/chat/completions` and parses `choices[0].message.content`; it does not send structured `tools` or parse `tool_calls`.
- The registry previously exposed tool definitions without capability checks, and the coordinator did not check structured-call support before dispatch.
- T0098–T0107 output a separate `Host.Windows/Requests/` request coordinator; their blocking relations remain in place as conservative sequencing.
- Added tests: missing provider, provider failure, unknown/unsupported/stale/empty-evidence report, actual text-only adapter report, gated advertisement, and dispatch denial before confirmation/worker launch. Tests run through HostLifecycle and a dedicated Windows workflow step.
- **CI pending:** these are source-review findings and test additions, not passed-test evidence. Local .NET execution was unavailable because the SDK is not installed. T0097 remains `in_progress`.
