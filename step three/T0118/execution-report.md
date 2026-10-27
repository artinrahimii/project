# T0118 execution report — fallback debugging in progress

- **Task:** add an alternate peer connection path without sending inference text through a rendezvous server.
- **Current attempt:** `T0118-A002`; A001's exact-head failure is retained.
- **Base implementation:** `V 3.0.356` — `27099fc451a383811823e3232ac49fce02fb4b8a`. The large graph Blob was retrieved fully and its Git SHA-1 recomputed from the original bytes matched; the fenced JSON was reconstructed exactly (229 tasks, 426 edges).
- **Implementation in V 3.0.356:** libp2p advertises TCP and QUIC/UDP addresses for the same PeerId. `PeerTransportFallbackConnector` tries an ordered candidate list, requires the same pinned peer identity for every address, disposes failed transports and returns the selected verified transport.
- **Failure observed on exact SHA:** HostLifecycle run [40448409783/job/122005094326](https://github.com/artinrahimii/project/actions/runs/40448409783/job/122005094326) installed `libmsquic` successfully and ran the test process, but the first refused TCP candidate escaped as `PeerConnectionException` with an inner `SocketException(Connection refused)`. The initial retry filter did not inspect nested exceptions, so the QUIC candidate was never attempted.
- **Fix in A002:** `IsRetryableConnectionFailure` now recursively inspects inner causes and AggregateException children, allowing a wrapped socket failure to advance to the next direct candidate. This does not change the pinned-identity rule or pass payloads through rendezvous.
- **Other evidence:** the Step Three quality gate passed on V 3.0.356: https://github.com/artinrahimii/project/actions/runs/40448409838. The product workflow was still in progress at the last query; no final product-build result is claimed here.
- **Limits:** the feature remains direct TCP→QUIC fallback only when UDP/QUIC is reachable. It is not NAT hole punching, circuit relay or a guarantee against firewalls blocking both paths. Those are not marked complete.
- **Status:** `in_progress`; rerun exact-head HostLifecycle and all product/quality workflows before considering acceptance.
