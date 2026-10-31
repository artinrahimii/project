# T0146 evidence ledger

- **Accepted implementation revision:** [`V 3.0.530`](https://github.com/artinrahimii/project/commit/ca45cc39b1153af79b7599c49bc0d9e473471375).
- **Quality workflow:** [40866732394](https://github.com/artinrahimii/project/actions/runs/40866732394) completed successfully: 225 Python tests, syntax checks, graph structure and evidence-path validation. Graph guard: 229 tasks, 417 edges, 0 errors, one preserved `T0001` free-text metadata warning.
- **Product workflow:** [40866732301](https://github.com/artinrahimii/project/actions/runs/40866732301) completed successfully.
- Android `ktlintCheck`, `assembleDebug`, `testDebugUnitTest`, generated Protobuf Java/Kotlin checks and APK upload passed.
- Windows WPF formatting, Release build, generated Protobuf C# check, startup smoke test, agent-tool process/policy/capability and DPAPI identity/trust checks passed; Windows artifact uploaded.
- HostLifecycle unit/integration tests passed.
- CI artifacts: `whitep-android-debug-apk` — 11,870,694 bytes; `whitep-windows-host` — 14,254,951 bytes.
- Protocol alignment: `protocol/whitep/v1/messages.proto` caps the prompt at 8,192 UTF-8 bytes; `src/Shared.Contracts/ProtocolMessageLimits.cs` matches that ceiling.
- The three failed attempts are preserved in graph history; the fourth attempt succeeded on V 3.0.530.
- Dependency review: T0145 → T0146 is a validated contract/interface dependency; T0138 → T0146 is sequencing-only. T0146 → T0147 remains unreviewed pending the next task's dependency review.
- **Not claimed:** host delivery, response generation, negotiated host-specific limits, encryption at rest, or independent-network P2P acceptance.
