# T0111 execution report — implementation awaiting exact-head CI

- **Task:** `T0111` — validate message size, protocol version, identifiers, deadline and permission to dispatch a request.
- **Attempt:** `T0111-A001` (partial; CI not yet verified).
- **Version intended for this implementation commit:** `V 3.0.340`.
- **Baseline:** `main` at `50e2d0605b1c0c41090f39c0673c0bebe373b382`. The graph Blob `0fc67b4190acfd21bbda9fe4ffb208c5a6b0f757` was fetched through Git Blob API after the normal file fetch returned empty content. The original UTF-8 byte length was 1,737,836 and the recomputed Git Blob SHA-1 matched the tree entry; the complete fenced JSON parsed to 229 nodes and 426 edges.
- **Implemented paths:** protocol source schema, shared validator/parser, host session call, gated listener, C# lifecycle/validation acceptance harness, Kotlin Protobuf round-trip test, Python schema/validator guards, and ADR-0003.
- **Behavior added:** required `InferenceRequest.protocol_version` wire field 8; exact `1.0.0` compatibility check; ASCII request/attempt identifiers bounded to 128 characters; invalid and expired deadline rejection with `PROTOCOL_ERROR_CODE_REQUEST_EXPIRED`; and a second local participation check after frame parsing and immediately before callback dispatch.
- **Wire compatibility:** existing field numbers 1–7 remain unchanged; the new optional Protobuf field uses previously unused field number 8. Old peers omitting it are rejected by this host validator until upgraded.
- **Security boundary:** the listener recheck authorizes local dispatch only while host participation is active. This does **not** establish cryptographic peer identity or authenticate the remote sender; the current request/transport contract does not supply a peer credential.
- **Dependency review:** `T0110 → T0111` is now `contract_or_interface_dependency` with source/test evidence; `T0107 → T0111` is `sequencing_only`. Both retain the original `blocks` edge type. The outgoing `T0111 → T0112` edge remains unreviewed.
- **Tests:** no test result is claimed until workflows run against the exact new commit. Step Three quality, Windows WPF build/HostLifecycle, Android Kotlin/Protobuf unit tests, and real local inference must be checked online.
- **Current state:** `in_progress`; acceptance is not yet declared complete.