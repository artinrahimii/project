#!/usr/bin/env python3
"""Black-box smoke tests for the packaged Rendezvous HTTP service container."""
from __future__ import annotations
import argparse
import json
import sys
import time
from datetime import datetime, timedelta, timezone
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode
from urllib.request import Request, urlopen

def call(base_url: str, method: str, path: str, payload: object | None = None) -> tuple[int, bytes]:
    body = None if payload is None else json.dumps(payload, separators=(",", ":")).encode("utf-8")
    headers = {"Accept": "application/json"}
    if body is not None:
        headers["Content-Type"] = "application/json"
    request = Request(base_url + path, data=body, headers=headers, method=method)
    try:
        with urlopen(request, timeout=5) as response:
            return response.status, response.read()
    except HTTPError as error:
        return error.code, error.read()

def require(condition: bool, message: str) -> None:
    if not condition:
        raise AssertionError(message)

def smoke(base_url: str, startup_timeout: float) -> None:
    deadline = time.monotonic() + startup_timeout
    healthy = False
    while time.monotonic() < deadline:
        try:
            status, body = call(base_url, "GET", "/healthz")
            if status == 200 and json.loads(body).get("status") == "ok":
                healthy = True
                break
        except (OSError, URLError, TimeoutError, json.JSONDecodeError, AttributeError):
            pass
        time.sleep(0.5)
    require(healthy, "The container did not become healthy at /healthz.")

    # Synthetic metadata only. TEST-NET address is not routable and does not imply reachability.
    peer_id = "WhitePContainerSmokePeer0123456789"
    announcement = {
        "peerId": peer_id,
        "addresses": [f"/ip4/203.0.113.10/tcp/4010/p2p/{peer_id}"],
        "capabilities": ["inference.v1"],
        "expiresAtUtc": (datetime.now(timezone.utc) + timedelta(minutes=5)).isoformat(),
    }
    status, _ = call(base_url, "POST", "/v1/peers/announcements", announcement)
    require(status == 204, f"Valid announcement returned HTTP {status}, expected 204.")

    query = urlencode({"capability": "inference.v1"})
    status, body = call(base_url, "GET", f"/v1/peers?{query}")
    require(status == 200, f"Capability discovery returned HTTP {status}, expected 200.")
    peers = json.loads(body)
    require(isinstance(peers, list), "Capability discovery must return a JSON array.")
    require(len(peers) == 1 and peers[0].get("peerId") == peer_id,
            "The announced peer was not returned by capability discovery.")

    invalid_query = urlencode({"capability": "not a capability"})
    status, _ = call(base_url, "GET", f"/v1/peers?{invalid_query}")
    require(status == 400, f"Invalid capability returned HTTP {status}, expected 400.")

    expired = dict(announcement)
    expired["expiresAtUtc"] = (datetime.now(timezone.utc) - timedelta(seconds=30)).isoformat()
    status, _ = call(base_url, "POST", "/v1/peers/announcements", expired)
    require(status == 400, f"Expired announcement returned HTTP {status}, expected 400.")

    invalid_address = dict(announcement)
    invalid_address["addresses"] = ["/ip4/203.0.113.10/tcp/4010"]
    status, _ = call(base_url, "POST", "/v1/peers/announcements", invalid_address)
    require(status == 400, f"Address without its peer identity returned HTTP {status}, expected 400.")
    print("PASS: non-root Rendezvous container health, announcement, discovery and invalid-input rejection.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--base-url", required=True)
    parser.add_argument("--startup-timeout-seconds", type=float, default=45.0)
    args = parser.parse_args()
    try:
        smoke(args.base_url.rstrip("/"), args.startup_timeout_seconds)
    except (AssertionError, OSError, URLError, TimeoutError, json.JSONDecodeError) as error:
        print(f"FAIL: {error}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
