# T0146 evidence ledger

- Start revision: [`V 3.0.526`](https://github.com/artinrahimii/project/commit/879aefc673b56916b4a35f1e41bb0d01fcdf2179).
- Implementation revision: [`V 3.0.527`](https://github.com/artinrahimii/project/commit/9b9d189cac8cdaecb0bc6a19739afa8a1a8f5733).
- Contract-test correction: [`V 3.0.528`](https://github.com/artinrahimii/project/commit/59fe16a2a60395f7eb7a3b21ce73719efce8a506).
- Protocol source: `protocol/whitep/v1/messages.proto` caps `InferenceRequest.prompt` at 8,192 UTF-8 bytes; `src/Shared.Contracts/ProtocolMessageLimits.cs` defines the matching ceiling.
- V 3.0.527 Step Three [40865663008](https://github.com/artinrahimii/project/actions/runs/40865663008) failed one stale Python assertion; Android product run [40865663105](https://github.com/artinrahimii/project/actions/runs/40865663105) failed Kotlin parsing in the VM test at line 96.
- V 3.0.528 Step Three [40865887712](https://github.com/artinrahimii/project/actions/runs/40865887712) passed 225 tests and graph guards (0 errors, 1 preserved warning); Android product run [40865887585](https://github.com/artinrahimii/project/actions/runs/40865887585) continued to fail on the test-file syntax defect before build/tests.
- V 3.0.529 restores the missing `=` before `runBlocking`; exact-head CI is pending.
- Edge review: T0145 → T0146 is a validated interface dependency; T0138 → T0146 is validated sequencing only. T0146 → T0147 remains unreviewed.
- **Not claimed:** transport delivery, negotiated host-specific limits, inference response generation, or independent-network P2P acceptance.
