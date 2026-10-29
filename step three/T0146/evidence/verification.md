# T0146 evidence ledger

- Start revision: [`V 3.0.526`](https://github.com/artinrahimii/project/commit/879aefc673b56916b4a35f1e41bb0d01fcdf2179).
- Protocol source contract: `protocol/whitep/v1/messages.proto` specifies a maximum of 8,192 UTF-8 bytes for `InferenceRequest.prompt`.
- Matching host-side ceiling: `src/Shared.Contracts/ProtocolMessageLimits.cs` defines `MaximumPromptUtf8Bytes = 8 * 1024` and counts UTF-8 bytes.
- Existing UI/data contract: `ConversationDetailScreen.kt`, `ConversationMessagesViewModel.kt`, `MessageEntity.kt`, and `ConversationMessagesViewModelTest.kt`; T0145 exact-head product and quality CI passed on V 3.0.526.
- Incoming edge review: T0145 → T0146 is a validated interface dependency; T0138 → T0146 is validated sequencing only. Both original `type=blocks` edges are preserved and neither is removed.
- New evidence: domain boundary tests cover blank input, exactly 8,192 UTF-8 bytes, over-limit input, and trimming; the ViewModel test checks that over-limit text is not persisted; Python source-contract checks connect UI/domain behavior to the protocol ceiling.
- Exact-head Android and Step Three workflow results are pending at the first implementation commit.
- **Not claimed:** transport delivery, host-specific negotiated limit handling, response generation, or independent-network P2P acceptance.
