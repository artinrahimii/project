# T0230 execution report — final-only production deployment and reachability

- **Task:** deploy the Rendezvous control plane behind public HTTPS and validate advertised peer reachability from an independent network.
- **Status:** `deferred_final` (not started, not complete, not counted as passing).
- **Why deferred:** this task needs a real hosting provider/domain, valid TLS termination, public routing and a peer on another network. None of these are required to continue implementation and automated test development.
- **Prerequisites:** T0121 renewable lease/lifecycle implementation and T0132 authenticated encrypted transport.
- **Execution order:** after ordinary implementation, automated regression and release-candidate preparation; before T0128 independent-node E2E acceptance.
- **Operational constraints:** use one service replica because the directory is in-memory; terminate TLS at a reverse proxy/ingress and do not expose the private HTTP container port directly. Configure the same real HTTPS origin in Windows and Android.
- **Required final evidence:** public HTTPS health check, redacted deployment identity/origin, announcement renewal and discovery by an independent peer, exact PeerId authenticated connection on advertised TCP/QUIC (where supported), then lease expiry/removal after publisher stop.
- **Security:** do not commit secrets, tokens, credentials or private keys. No deployment, endpoint or independent-network test was executed by this deferral change.
