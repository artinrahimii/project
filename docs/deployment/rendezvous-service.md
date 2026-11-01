# Rendezvous service container deployment

This is the metadata-only rendezvous control plane for Windows hosts and Android clients.
It stores peer IDs, direct multiaddresses, capabilities, and lease expiry in process memory;
it does not carry inference prompts or model responses.

## Build and run

Build from the repository root so the service can restore its shared-contract project:

```sh
docker build -f src/Rendezvous.Service/Dockerfile -t whitep-rendezvous:local .
docker run --detach --name whitep-rendezvous --restart unless-stopped \
  --publish 127.0.0.1:8080:8080 whitep-rendezvous:local
curl --fail http://127.0.0.1:8080/healthz
docker logs whitep-rendezvous
docker rm --force whitep-rendezvous
```

The image listens on port `8080`, runs as UID/GID `10001` rather than root,
and exposes `GET /healthz` for platform health probes.

The container intentionally serves HTTP on its private application port. A public
deployment must sit behind an ingress or reverse proxy that terminates TLS, serves
a valid certificate for its public DNS name, forwards only to the private container
port, and applies provider-level rate and connection limits. Never expose the
container's HTTP port directly to the public Internet.

White-P clients require the public rendezvous origin to use HTTPS, for example
`https://rendezvous.example.net/`. This is a placeholder, not a deployed endpoint.
Configure the same real HTTPS origin in Windows and Android clients. Do not commit
deployment credentials or production secrets.

## State and scaling constraint

`PeerAnnouncementDirectory` is a singleton in-memory directory. Run exactly one
service replica for this release. Multiple replicas have independent peer lists and
can return inconsistent discovery results. A restart clears the directory; hosts
must announce again and resume lease renewal. Do not enable horizontal scaling,
scale-to-zero or multi-replica rolling deployment until a shared, expiry-aware store
and concurrency semantics are implemented and tested.

The publisher defaults to a five-minute TTL and one-minute renewal. A stopped publisher
is removed by expiry filtering when the next directory request runs. A metadata lease
does not prove that the advertised peer address accepts inbound TCP or QUIC.

## Public reachability acceptance remains mandatory

A successful image build, container smoke test, health probe or loopback request does
not verify public reachability. Before marking the P2P announcement task complete:

1. Deploy this image to a real host with a public HTTPS rendezvous origin.
2. Configure a Windows host with a real, stable libp2p identity and direct public
   TCP/QUIC multiaddresses actually mapped to that host.
3. From a peer on another network, retrieve the announcement and establish an
   authenticated peer connection to the exact advertised PeerId.
4. Verify TCP/QUIC fallback, record observable results, then stop the publisher and
   verify the lease expires and the peer disappears from discovery.
5. Retain redacted evidence. Never include private keys, credentials or access tokens.

Until this test passes, public advertised-address reachability remains unverified.
