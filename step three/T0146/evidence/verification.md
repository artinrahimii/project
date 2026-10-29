# T0146 evidence ledger

- Start revision: [`V 3.0.526`](https://github.com/artinrahimii/project/commit/879aefc673b56916b4a35f1e41bb0d01fcdf2179).
- Implementation: [`V 3.0.527`](https://github.com/artinrahimii/project/commit/9b9d189cac8cdaecb0bc6a19739afa8a1a8f5733); Python contract correction: [`V 3.0.528`](https://github.com/artinrahimii/project/commit/59fe16a2a60395f7eb7a3b21ce73719efce8a506); Kotlin test syntax correction: [`V 3.0.529`](https://github.com/artinrahimii/project/commit/d61ffa7996951a9e06d1b8012016970a367c9042).
- Protocol source `protocol/whitep/v1/messages.proto` sets the prompt cap at 8,192 UTF-8 bytes; host-side `ProtocolMessageLimits.MaximumPromptUtf8Bytes` matches it.
- V 3.0.529 Step Three gate [40866241718](https://github.com/artinrahimii/project/actions/runs/40866241718) passed 225 Python tests, syntax, and graph/evidence guards (229 tasks / 417 edges, 0 errors and one preserved T0001 metadata warning).
- V 3.0.529 product run [40866241861](https://github.com/artinrahimii/project/actions/runs/40866241861) passed Windows WPF and HostLifecycle jobs. Android ktlint failed at `ConversationDetailScreen.kt:79:29` with “A multiline expression should start on a new line,” so APK build and JVM tests were not executed.
- V 3.0.530 moves `remember(draft)` to the next line after `=`; exact-head CI is pending.
- Dependency review: T0145 → T0146 is a validated interface dependency; T0138 → T0146 is validated sequencing-only. T0146 → T0147 remains unreviewed.
- **Not claimed:** network delivery, negotiated host-specific limits, response generation, or independent-network P2P acceptance.
