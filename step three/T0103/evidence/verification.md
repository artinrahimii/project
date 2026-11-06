# T0103 verification evidence

- **Repository/base:** `artinrahimii/project`, branch `main`, base commit `39d793913731f22aeef6ed2eff15f3878b0b8239` (`V 3.0.318`).
- **Dependency:** T0097 and T0102 were completed with evidence before this attempt. The edge T0102 → T0103 was inspected and recorded as `sequencing_only`, retaining `type=blocks` to preserve the existing order.
- **Implementation:** `HostRequestCoordinator.CancelActiveRequest()` cancels a linked token supplied to the active operation callback and releases the single-request gate on cancellation/exception. Existing deadline checks and coordinator admission remain in place.
- **Inference/network:** the real `LlamaCppInferenceEngine` source is compiled into the HostLifecycle harness; tests drive model discovery and a pending chat-completion request through the actual engine/backend code. The cancellation probe records whether the token passed to `HttpClient.SendAsync` is cancelled.
- **Automated acceptance sources:** `HostRequestCoordinatorTests` and new `LlamaCppInferenceEngineCancellationTests` cover coordinator cancellation, cancellation of the operation token, propagation to the HTTP call, engine-level `CancelAsync`, and gate release. Exact-head CI results remain pending at report creation.
- **Protocol:** no Protobuf field or field number changed.
- **Limitations:** no local .NET SDK is available; no local C# test/build is claimed. HTTP cancellation reaching the client transport is not a guarantee that remote model computation stops immediately. No production P2P request ingress/response sender is claimed.


## Exact-head verification — T0103-2026-10-26-02

- Tested commit: `e6022b14b1bbf4cb00fe23da406c4c65d965047e` (`V 3.0.319`) on `main`.
- HostLifecycle cancellation acceptance passed in job `121560350510`: coordinator API cancels the active token, cancellation reaches the HTTP request through `LlamaCppInferenceEngine`, and `IInferenceEngine.CancelAsync` cancels active generation.
- Step Three quality gate passed: https://github.com/artinrahimii/project/actions/runs/40319456864
- Windows C# formatting, Release build, Protobuf generation, startup smoke and other Windows gates passed: https://github.com/artinrahimii/project/actions/runs/40319456776/job/121560350429
- Android lint/formatting, debug APK, Protobuf round-trip tests, generated-source checks and artifact upload passed: https://github.com/artinrahimii/project/actions/runs/40319456776/job/121560350171
- Real local inference workflow passed on the same implementation commit: https://github.com/artinrahimii/project/actions/runs/40319456842
- No protobuf fields/numbers changed. No production P2P ingress or response sender is claimed.
