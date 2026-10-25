# T0086 — Execution report

- Base commit: \`9e12e03b34e3f0f154e8aae79a05ce9d16f032f4\` (\`V 3.0.258\`), branch \`main\`.
- Preconditions: T0085 and T0075 are done with verified evidence. Reviewed the incoming edges \`T0085 -> T0086\` and \`T0075 -> T0086\` against the current installer/catalog contracts. Both are concrete interface/contract dependencies; their existing \`type=blocks\` edges remain unchanged.
- Production change: \`VerifiedModelInstaller\` now rejects non-positive declared sizes and checks actual free bytes on the destination volume before network access or staging-file creation. An injectable free-space provider permits deterministic insufficient-space tests without filling a real disk. The default implementation queries \`DriveInfo.AvailableFreeSpace\` for the destination volume and fails closed if capacity is unavailable.
- New \`UpdateModelSafetyAcceptanceTests\`: (1) insufficient free space refuses the update before HTTP and leaves the previous file unchanged, (2) HTTP 503 keeps the previous file and leaves no temp/backup artifact, (3) malformed SHA-256 is rejected before networking, and (4) cancellation after actual partial download progress cleans the staging file and preserves the previous model byte-for-byte.
- New removal regression: \`DisconnectWaitsForGenerationBeforeDisposalAsync\` starts an in-flight request, requests disconnect/removal, proves the backend remains undisposed while generation is in flight, then releases generation and checks stopped/disposed/not-connected state.
- Existing coverage remains registered for interrupted network reads, partial-download cancellation, checksum mismatch, atomic replacement, and ordinary backend disposal.
- Task status is **in progress** until the new exact-head quality gate, HostLifecycle suite and Windows build actually pass. No new test result is claimed yet.
- Next action: inspect CI for the versioned commit, fix failures if any, record each attempt, and only then mark T0086 done.
