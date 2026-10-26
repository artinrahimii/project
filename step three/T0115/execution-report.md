# T0115 execution report — in progress, awaiting exact-head CI

- **Task:** add acceptance tests for a disconnected connection, malicious request, repeated connections and shutdown.
- **Attempt:** `T0115-A001`.
- **Baseline:** `main` at `e05efbcc6881599a7ccec40975ea72624a921111` (`V 3.0.346`); graph contains 229 tasks and 426 edges.
- **Incoming dependency review:** `T0114 → T0115` and `T0107 → T0115` were reviewed as `sequencing_only`. T0115 exercises the existing listener lifecycle and coordinator-adjacent boundary but does not consume a new API from either task; both `blocks` edge types are preserved with evidence and rationale.
- **Tests added:** a client that disconnects before sending its frame must release its slot and not prevent a later valid request; a semantically invalid framed Protobuf request must be rejected before callback; two sequential valid connections must each be processed once while the listener remains active. Existing tests continue to cover malformed/oversized/truncated frames, permission revocation and shutdown while a client waits for a frame.
- **Tests:** exact-head CI pending. `T0115` remains `in_progress` until HostLifecycle and quality gates pass on the implementation commit.