# T0120 verification evidence — assertion helper fix pending CI

## Baseline
- Repository `artinrahimii/project`, `main`.
- Latest code-test attempts before fix: `V 3.0.361` (`9985bb108c02226f1817b6d6cd2562467703e487`) and `V 3.0.362` (`7d74171cec228e534f2700f0afc3c5085bf419e5`).
- The graph at both commits was retrieved by Git Blob API, its Git SHA-1 matched the complete original bytes, and its fenced JSON reconstructed exactly: 229 tasks, 426 edges.

## Observed failure history
- V 3.0.361 failed before integration because the capability test input did not reliably create a control character.
- V 3.0.362 corrected that input. The production contract rejected the explicit newline, but `ExpectArgumentException` had its return in the wrong branch: it fell through after catching the expected `ArgumentException` and threw the assertion failure. The loopback HTTP tests still had not run.
- Evidence: https://github.com/artinrahimii/project/actions/runs/40451885440 and https://github.com/artinrahimii/project/actions/runs/40452192247.

## Correction
- `ExpectArgumentException` now returns only when `ArgumentException` is caught. Normal return from the action results in an assertion failure.
- Exact V 3.0.363 build/runtime/quality CI is pending. Do not claim service acceptance until the loopback announce/discover/replacement, unknown-field rejection, no-inference-route and request-size checks actually execute and pass.
