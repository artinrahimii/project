# T0041 evidence — verified WPF entry point

- Tested commit: `d5fe343a96c524625ee650792f3548330152ae7f` (`V 3.0.123`).
- [Step Three quality gate run 39862296293](https://github.com/artinrahimii/project/actions/runs/39862296293): passed.
- [White-P product build run 39862296386](https://github.com/artinrahimii/project/actions/runs/39862296386): passed.
- The product workflow builds the Windows WPF executable, runs its bounded `--smoke-test`, and uploads the artifact only on success; the same workflow runs the C# host test harness and Android build/tests.
- Remaining product limitations: live inference with an installed model, hardware discovery, Android-to-host networking, and direct P2P transport remain unverified or unimplemented.
