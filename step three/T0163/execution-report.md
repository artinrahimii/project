# T0163 execution report

- **Task:** Send one correlated inference attempt to each selected host using a shared ephemeral request ID and a distinct attempt ID per host.
- **Last verified implementation revision:** `main` commit `4a912bef939384f254c844841a3d54656af68306` (`V 3.0.623`).
- **Graph source integrity:** `step three/project-graph.md` was fetched from the exact V 3.0.623 tree Blob `81ae04a2ee54e01ff725a84ffd3031de8ac043e6`; tree size and fetched content both measured 3,028,113 UTF-8 bytes; recomputed Git Blob SHA-1 matched the exact Blob ID. Complete fenced JSON parsed as 229 nodes and 417 edges, with unique task IDs and no dangling edge references.
- **Predecessors and dependencies:** T0160 and T0162 are `done`. T0160 → T0163 and T0162 → T0163 are validated `contract_or_interface_dependency` edges with rationale/evidence; original `type: blocks` and execution order are preserved.
- **Implementation:** `InferenceAttemptDispatcher` validates the non-terminal request, at most three unique and identity-consistent host candidates, unique nonblank attempt IDs and every payload before any transport call. Each selected host gets a canonical Protobuf `InferenceRequest` that shares the distributed request ID and has a distinct per-host attempt ID. The dispatcher uses only the explicitly injected `InferenceRequestTransport`; it has no mock/default transport.
- **Android tests:** Four blank-prompt tests independently prove rejected result type, `PAYLOAD_REJECTED` category, `EMPTY_PROMPT` cause, and zero host submissions. A fixture correction made its normal attempt-ID generator unique; a separate test deliberately injects duplicate IDs to verify rejection.
- **V 3.0.623 CI:** 271 Python quality tests passed, compileall and graph/evidence checks passed (zero errors; one preserved T0001 metadata warning). Android lint, debug APK build, all 84 JVM tests, Protobuf Java/Kotlin verification and APK artifact upload passed. Windows WPF formatting/build/Protobuf/startup/security/DPAPI/artifact and HostLifecycle passed. Full logs and artifact digests are in `evidence/v3.0.623-ci-final.md`.
- **History:** Nine earlier attempts (A001–A009) are preserved as failed, including KtLint corrections, graph attempt-count consistency, missing Kotlin test import, test contract name update, and diagnosing a test fixture's repeated default attempt ID. A010 succeeded for its verified per-host dispatch boundary; all nine earlier failed attempts remain in the append-only history.
- **Important remaining integration:** No concrete Android-to-Windows transport adapter or routable authenticated peer-address resolver is wired into the client. The implementation invokes a transport port; that proves the dispatch contract and JVM behavior, not delivery to a remote host, peer authentication, inference completion, or real independent-network reachability.
- **Task status:** `in_progress`, not `done`. The next required work is the concrete authenticated Android-to-host transport/address resolution and a real-host end-to-end delivery test. T0164 remains the logical next task after T0163 is genuinely complete; it is not started or unblocked by this commit.


## Attempt T0163-A011 — partial implementation; exact-head CI pending

- **Started / recorded:** 2026-10-30T14:09:27.679Z.
- **Base revision:** `main` commit `434d0fe11bd027695185d1fec0874d1cfa1f47f8` (`V 3.0.624`).
- **Protocol evidence:** Windows `Libp2pPeerMessageProtocol` uses the application protocol `/white-p/peer-message/1.0.0`, reads a varint length followed by one complete message, and its transport default is 4 MiB. The shared request contract separately caps serialized inference requests at 16 KiB.
- **Implementation added:** Android `PeerMessageFrameCodec` encodes canonical unsigned 32-bit varint length prefixes and `PeerMessageFrameDecoder` incrementally reassembles split prefixes/payloads, emits complete messages, bounds each declared frame before payload copying, and rejects overlong/non-canonical/overflow prefixes and truncated stream endings.
- **Tests added:** Seven Android JVM tests cover 127/128-byte prefix boundaries, fragmented prefix and payload reads, multiple frames including an empty message, oversize rejection before payload arrival, malformed varints, truncated payloads, invalid limits, and encoder limits. The Python source-contract gate now requires the codec and its boundary tests.
- **Local execution:** Kotlin/Gradle and Python tests were not run in this tool session. A local repository clone could not be obtained because the runtime could not resolve `github.com`; exact-head GitHub Actions is the verification path after the online commit.
- **Scope boundary:** This is the real Android framing component only. It does not create a libp2p host, open TCP/QUIC sockets, authenticate a remote PeerId, map `ephemeralHostId` to PeerId/multiaddress, bind the dispatcher to a concrete transport, or prove remote receipt/inference. No claim of end-to-end Android-to-Windows delivery is made.
- **Task status:** `in_progress`; this attempt is `partial`, not `done`. The next required integration is authenticated Android libp2p session establishment plus verified directory identity/address resolution, followed by cross-platform loopback and independent-host acceptance.


## Attempt T0163-A011 — exact V 3.0.625 CI result

- **Commit:** `0cb19f09f9f7d424f0abec59d087bfacffb704c6` (`V 3.0.625`) on `main`.
- **Step Three quality gate:** PASS — 273 Python tests; compileall; graph guard 229 tasks / 417 edges, zero errors and one preserved T0001 metadata warning. [Workflow](https://github.com/artinrahimii/project/actions/runs/41069596957).
- **Android KtLint:** FAILED — import ordering in `PeerMessageFrameCodecTest.kt:3` and multiline expression layout at line 42. [Product workflow](https://github.com/artinrahimii/project/actions/runs/41069596909).
- **Skipped:** Android APK build, Android JVM tests, generated Protobuf check and APK artifact upload were skipped by the lint gate. No Android JVM result is claimed.
- **Windows WPF and HostLifecycle:** PASS — C# formatting/build/security/startup checks and lifecycle unit/integration step completed successfully.
- **Outcome:** The code and test sources are retained, but A011 is recorded as a failed verification attempt. T0163 remains `in_progress`.

## Attempt T0163-A012 — KtLint corrections; exact-head CI pending

- **Recorded:** 2026-10-30T14:14:17.184Z; base commit `0cb19f09f9f7d424f0abec59d087bfacffb704c6` (`V 3.0.625`).
- **Correction:** Reordered the new JVM test imports to the repository's required lexicographic order and moved the concatenated-frame expression to the expected multiline initializer layout.
- **Scope:** Formatting-only correction; production codec behavior, frame format, maximum size, and acceptance cases are unchanged.
- **Local execution:** Kotlin/Gradle and Python tests were not run locally. Exact-head GitHub Actions for `V 3.0.626` is pending.
- **Task status:** `in_progress`; this attempt is `partial` pending KtLint, APK, JVM and product workflow results. The missing authenticated libp2p client and trusted rendezvous identity/address mapping remain unresolved.
