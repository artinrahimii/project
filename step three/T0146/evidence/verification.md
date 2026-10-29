# T0146 evidence ledger

- Start revision: [`V 3.0.526`](https://github.com/artinrahimii/project/commit/879aefc673b56916b4a35f1e41bb0d01fcdf2179).
- Implementation revision: [`V 3.0.527`](https://github.com/artinrahimii/project/commit/9b9d189cac8cdaecb0bc6a19739afa8a1a8f5733).
- Protocol source contract: `protocol/whitep/v1/messages.proto` limits `InferenceRequest.prompt` to 8,192 UTF-8 bytes.
- Matching host-side ceiling: `src/Shared.Contracts/ProtocolMessageLimits.cs` defines `MaximumPromptUtf8Bytes = 8 * 1024` and counts UTF-8 bytes.
- The V 3.0.527 Step Three quality gate [40865663008](https://github.com/artinrahimii/project/actions/runs/40865663008) failed one stale assertion in `tests/test_android_conversation_detail_contract.py`; the code intentionally replaced the inline check with `ConversationMessageValidator.validate(content)`.
- V 3.0.528 updates that assertion to verify the validator contract. Exact-head verification for the correction is pending.
- Incoming edge review: T0145 → T0146 is a validated interface dependency; T0138 → T0146 is validated sequencing only. Both `type=blocks` edges remain present; T0146 → T0147 stays unreviewed.
- **Not claimed:** transport delivery, negotiated host-specific limits, response generation, or independent-network P2P acceptance.
