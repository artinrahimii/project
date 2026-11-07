# شواهد ممیزی اولیه UPD-S1-003

## مبنای دقیق

- Commit: `38c63f441cd3991da105ba1edafc319c30dc7241` — `V 3.1.26`.
- Update graph Blob SHA at this commit: `00aa4e806ba5b3af7fa3e37d641df4121aba88de`.
- Requirements V1.10 Blob SHA: `5cea3ec095d0c00e32ed3e067796ff698aedd982`.
- Routing proposal file Blob SHA: `cbf87a7ff5b91b29e6ecda19020fd16119c7003f`.
- Source blob SHAs reviewed:
  - `DistributedRequest.kt`: `a7fb3594520ece59418dfa0ef3d0660a6b59550c`.
  - `HostCandidateSelector.kt`: `e32db4d192879f3d77cd46603d3c60ceb1b5c2db`.
  - `InferenceAttemptDispatcher.kt`: `99f69fbadc0270aec5ed6464c3f4cbb383d2417f`.
  - `InferenceRoundResponseCollector.kt`: `ffb9a336e083d2e1317efe8a0f88ab286a36df09`.
  - `InferenceRoundTimer.kt`: `e8ff38ce19be0a115fb3a322f59829f741f5c94e`.
  - `RequestViewModel.kt`: `25f0f018976ab9a2645d0d28e79f9e92eebdac58`.

## Code-backed facts

- First-release limits are currently three rounds, three selected hosts per round, nine distinct attempts budget in default logical request.
- Candidate selector sorts peers and caps a round at 3.
- Discovery does live authenticated probes; it stops when maxHosts candidates are selected.
- Dispatcher sends an independent correlated attempt to each selected peer.
- Collector awaits attempt outcomes until a shared round response deadline.
- Per-round timeout default is 60 seconds.
- Request view-model total deadline currently 90 seconds and is extended on manual retry.
- Manual retry is explicitly initiated by user; new attempt and round IDs are produced; logical request ID is reused.
- The UI currently offers cancel for every queued selected attempt; the Windows receive loop cancels only the exact matching request/attempt.
- Disconnect/lifecycle shutdown cancels active request handlers and drains work.
- Late/duplicate responses are correlated and rejected by protocol validators / response collection logic.
- The new proposal section 8.2 recommends one host first, then another after previous completion or safe cancellation; this is not yet encoded as a product decision.
- Generic three-host selector/dispatcher/collector infrastructure can be preserved behind a future/compatibility scope even if the first-release request UI is limited to one active host.

## Legacy task inventory

Affected IDs extracted from UPD-S1-003:
`T0125, T0126, T0161, T0162, T0163, T0164, T0165, T0166, T0167, T0168, T0169, T0170, T0171, T0172, T0173, T0174, T0175, T0176, T0177, T0203, T0204, T0224, T0225`.

Old graph facts:
- T0125–T0126: done.
- T0161–T0177: done.
- T0203–T0204: done.
- T0224–T0225: not_started, no history.

No old status or history is modified in this task.

## Scope limits

- No physical Android/Windows devices accessed.
- No product build or test execution in this initial audit attempt.
- No policy decision accepted yet.
