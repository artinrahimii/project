# T0110 execution report — accepted

- **Status:** Done on `V 3.0.338` — [commit](https://github.com/artinrahimii/project/commit/86267255efa01fe899a26d58f5e5beec4f860c75).
- **Attempt 01:** Failed only after the new framing test reached `AssertPeerClosedAsync`: an already disconnected `TcpClient.GetStream()` threw `InvalidOperationException`. Historical failure remains preserved in the graph.
- **Attempt 02:** Fixed the helper to accept an already disconnected socket during stream acquisition but keep the `read == 0` assertion outside that catch. The full exact-head suite then passed.
- **HostLifecycle and framing acceptance:** [PASS](https://github.com/artinrahimii/project/actions/runs/40406054183/job/121863964765). Explicit log line: `PASS: bounded TCP frames reach the shared Protobuf parser; malformed, truncated, oversized and interrupted frames are rejected.`
- **Step Three quality gate:** [PASS](https://github.com/artinrahimii/project/actions/runs/40406054261).
- **Windows Release build/startup smoke and Android lint/APK/Protobuf/artifact steps:** [PASS](https://github.com/artinrahimii/project/actions/runs/40406054183).
- **Real local llama.cpp/GGUF inference acceptance:** [PASS](https://github.com/artinrahimii/project/actions/runs/40406054061).
- The original graph Blob at V336 was `355f2972c2e9ad3acd4c022644440d55feb17748`, measured 1,718,063 UTF-8 bytes / 1,596,320 Unicode characters; its recomputed Git Blob SHA-1 matched. The V338 graph Blob before this final status update was `2bfd0becbf7f7f813f0a4efb4a1ec2f339e2c8ed`, measured 1,732,522 UTF-8 bytes / 1,610,882 Unicode characters; its recomputed SHA-1 also matched. Both complete fenced JSON versions parsed successfully.
- **Implementation boundary:** the frame is one serialized Protobuf `InferenceRequest` per TCP connection, with 4-byte unsigned big-endian payload length and 16,384-byte maximum checked before allocation. Valid requests reach an optional callback; no callback is wired to model inference and no network response is claimed. Android/peer transport must honor the same framing or document a reviewed protocol change.
- **Next:** review `T0110 -> T0111` before starting T0111; it remains `unreviewed`.
