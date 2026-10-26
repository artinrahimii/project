# T0115 verification evidence — pending exact-head CI

## Baseline and edge review
- Repository `artinrahimii/project`, branch `main`, base `e05efbcc6881599a7ccec40975ea72624a921111`.
- Graph Blob `f33f7e9672b1721963c2bec64a7fcc729911d761` parsed to 229 tasks / 426 edges.
- `T0114 → T0115`: reviewed as `sequencing_only`; the tests exercise the existing listener's behavior but consume no new T0114-specific API. `blocks` retained.
- `T0107 → T0115`: reviewed as `sequencing_only`; the tests are in the TCP listener acceptance boundary and consume no new request coordinator artifact. `blocks` retained.

## Acceptance coverage
- `DisconnectedClientDoesNotPreventSubsequentValidRequestAsync`: closes a real loopback TCP client before it sends a frame, waits for active client count to return to zero, and then verifies a later valid request is still delivered.
- `SemanticallyInvalidRequestIsRejectedAsync`: sends a properly framed Protobuf request with an unsafe request ID and asserts the handler callback is never called.
- `SequentialConnectionsAreHandledIndependentlyAsync`: verifies two valid one-frame connections are processed once each and the listener remains active after each client is closed.
- Existing acceptance methods continue to cover malformed and oversized frames, truncated prefix/payload, participation revocation, privacy and shutdown of a client blocked waiting for its frame.

## Verification
- Exact-head HostLifecycle and Step Three quality jobs: **pending**.
- Do not mark `T0115` done until these test results pass on the new commit.
