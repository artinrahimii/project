
# T0162 verification evidence — exact V 3.0.611 CI audit

- **Implementation candidate:** `d0b02ae8f4e927ba2b373567b2b1c39d184970ea` (`V 3.0.611`).
- **Graph baseline:** exact-tree Blob `279534dbbd6fdc36d3950d2cdb542f67b8c70d65`; size 2,973,531 UTF-8 bytes; recomputed Git SHA-1 matched; complete fenced JSON parses as 229 nodes / 417 edges.
- **Step Three quality gate — FAIL:** https://github.com/artinrahimii/project/actions/runs/41040367408. 268 tests ran; 267 passed, one source-contract test failed because it expected the superseded direct prompt conversion expression.
- **HostLifecycle — PASS:** https://github.com/artinrahimii/project/actions/runs/41040367617 (job 124011378288).
- **Android KtLint — PASS; debug APK — PASS:** https://github.com/artinrahimii/project/actions/runs/41040367617 (job 124011378514).
- **Android JVM — FAIL:** `HostCandidateSelectorTest.kt` used `InferenceRequestPayloadBuilder` constants without importing `com.whitep.client.protocol.InferenceRequestPayloadBuilder`; compilation failed with unresolved references at lines 106 and 108. Generated Protobuf verification and APK upload were skipped.
- **Windows WPF — PASS:** https://github.com/artinrahimii/project/actions/runs/41040367617 (job 124011378610), all configured steps including DPAPI and agent-tool policy checks completed successfully.
- **A004 correction:** align the Python source-contract test with the new production locals and add the missing Kotlin import. No production source behavior changed.
- **Task state:** `T0162` remains `in_progress`. Its JVM behavioral tests have not yet executed successfully on CI; do not mark done.
