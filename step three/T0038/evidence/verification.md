# T0038 verification evidence

## Failed exact-commit verification retained

- Commit `20832c67a78a04841f8e6f2ef81969e099603791` (`V 3.0.112`).
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/39851178922): passed. This is repository/guard verification, not cross-language fixture acceptance.
- [C# product job](https://github.com/artinrahimii/project/actions/runs/39851178925/job/120077455638): failed while parsing the `HostCapabilities` fixture with `InvalidProtocolBufferException: invalid tag (zero)`.
- [Android product job](https://github.com/artinrahimii/project/actions/runs/39851178925/job/120077456458): ktlint failed; the APK and Kotlin unit tests were skipped.

## Corrective attempt

The fixture corpus and Kotlin test formatting have been corrected in the current attempt. The exact-commit product run has not completed yet; no passing C#/Kotlin shared fixture test is claimed here until CI evidence is recorded.


## Attempt A003 result
- Commit `17cd09a637aedf989bd6337b91cdf4552265c022`: quality gate, C# host tests/shared-fixture parsing and Windows WPF build passed.
- Android ktlint failed only because `ProtoRoundTripTest.kt` had no trailing newline; Kotlin unit tests and APK build were skipped.
- This is a failed verification attempt, not acceptance. A004 adds the exact newline fix; its CI result is pending.
