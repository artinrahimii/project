# T0128 execution report — deferred to final independent-network acceptance

- **Task:** run end-to-end acceptance on two real computers or independent network nodes.
- **Status:** `deferred_final` (not started, not complete, not counted as passing).
- **Reason:** this acceptance requires independent computers/networks; repository-only CI and loopback tests cannot prove real advertised-address reachability.
- **Prerequisites:** T0127, T0115 and T0131; plus T0230 public HTTPS Rendezvous deployment and advertised TCP/QUIC reachability evidence.
- **Execution order:** final operational queue, after T0230.
- **Attempts/tests:** none in this change. No external peer, public endpoint or successful network session is claimed.
- **Required evidence:** redacted endpoint/health result, peer discovery for exact PeerId, authenticated connection over advertised transport, request/response exchange, and expiry/removal after publisher stop. Store in `step three/T0128/evidence/verification.md` when actually executed.
