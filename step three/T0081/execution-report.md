# T0081 execution report

- Task: \`T0081\` — finalize installation only after checksum and file-format validation.
- Target branch: \`main\`; base commit: \`75e7beba9bf369046d4bb6dbd088a40df765dce9\` (\`V 3.0.246\`).
- Dependency review: \`T0080 -> T0081\` and \`T0075 -> T0081\` keep \`type=blocks\` and have been validated as \`sequencing_only\`; format validation directly consumes the verified staged file/catalog metadata and does not directly call either the cancellation test artifact or offline catalog store. The required conservative order remains unchanged.
- Implementation: \`VerifiedModelInstaller\` rejects declared formats other than \`GGUF\` before network access. After download, it checks SHA-256 first, then reads the first 8 bytes of the staged file and validates the \`GGUF\` magic and supported header version (1–3); only then does it promote the file to its final name.
- Tests: extended \`VerifiedModelInstallerAcceptanceTests\` with checksum-correct malformed-magic and unsupported-version cases, plus rejection of an unsupported declared format before network access. Negative cases must leave no final or staging file.
- Status: \`in_progress\`; exact-head graph quality gate and HostLifecycle tests are pending.


## Attempt 2 — checksum then format validation passed

- Implementation commit: `96887ab62ae49d16eb1abd052464491054f194a1` (`V 3.0.247`).
- Step Three quality gate [run 40162939731](https://github.com/artinrahimii/project/actions/runs/40162939731) passed.
- HostLifecycle job [121078109102](https://github.com/artinrahimii/project/actions/runs/40162939679/job/121078109102) passed. The registered installer acceptance suite validates all positive/negative cases before emitting its PASS line, including checksum-correct invalid GGUF magic, unsupported GGUF version, and unsupported declared format with no network request.
- The full Windows/Android product build was still in progress when this task result was recorded and is not claimed as complete here.
- Task result: `done` for checksum-plus-format gate before model promotion.
