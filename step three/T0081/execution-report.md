# T0081 execution report

- Task: \`T0081\` — finalize installation only after checksum and file-format validation.
- Target branch: \`main\`; base commit: \`75e7beba9bf369046d4bb6dbd088a40df765dce9\` (\`V 3.0.246\`).
- Dependency review: \`T0080 -> T0081\` and \`T0075 -> T0081\` keep \`type=blocks\` and have been validated as \`sequencing_only\`; format validation directly consumes the verified staged file/catalog metadata and does not directly call either the cancellation test artifact or offline catalog store. The required conservative order remains unchanged.
- Implementation: \`VerifiedModelInstaller\` rejects declared formats other than \`GGUF\` before network access. After download, it checks SHA-256 first, then reads the first 8 bytes of the staged file and validates the \`GGUF\` magic and supported header version (1–3); only then does it promote the file to its final name.
- Tests: extended \`VerifiedModelInstallerAcceptanceTests\` with checksum-correct malformed-magic and unsupported-version cases, plus rejection of an unsupported declared format before network access. Negative cases must leave no final or staging file.
- Status: \`in_progress\`; exact-head graph quality gate and HostLifecycle tests are pending.
