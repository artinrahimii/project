# T0167 — multiple-host response rendering evidence

## Prerequisite and edge evidence

- T0166 and T0160 are done.
- T0166 → T0167 is a validated `contract_or_interface_dependency`: the renderer consumes the per-attempt response state and Compose card interface.
- T0160 → T0167 is a validated `sequencing_only` edge: the response renderer does not call or consume the request payload builder. The original blocks ordering remains.

## Production rendering evidence

- `HostResponseCards.kt` iterates the response list and renders one Material 3 Card per element.
- Each card reads its own host, attempt, model, outcome, output and error fields.
- Each subtree is now keyed by `(requestId, attemptId)`, keeping same-host retries distinct in Compose identity.
- No response string joining or aggregate output is used.

## Automated regression evidence

- New test: `tests/test_android_multi_host_response_rendering_contract.py`.
- It checks the per-response loop, the stable request/attempt key, separate Card/output structure and absence of concatenation.
- Existing JVM test `keepsEachCorrelatedHostAttemptAsAnIndependentResponseCard` asserts two entries, distinct output strings and attempt IDs; V 3.0.698 exact-head Android JVM CI passed.

## Scope boundary

This task covers response presentation only. The request screen still identifies Android-to-host request submission as unavailable; it does not prove a complete live multi-host inference round.
