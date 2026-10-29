# T0144 evidence ledger

- Initial implementation: [`V 3.0.517`](https://github.com/artinrahimii/project/commit/12b76f6f617c982b1353e8e6ec7cb7f5370bb6b3).
- First correction: [`V 3.0.518`](https://github.com/artinrahimii/project/commit/06b8dd63afa37902ea9b834e97abe80458f6375b).
- Exact-head quality gate on V 3.0.518: passed, 218 Python tests and graph guard; https://github.com/artinrahimii/project/actions/runs/40851894119.
- Exact-head HostLifecycle tests on V 3.0.518: passed; https://github.com/artinrahimii/project/actions/runs/40851894117/job/123373165089.
- Android ktlint on V 3.0.518: failed on three formatting rules; APK build and Android unit tests were skipped. https://github.com/artinrahimii/project/actions/runs/40851894117/job/123373165453.
- Attempt A003 modifies only the residual lint issues and synchronizes the graph/history. Its exact-head CI result remains pending.
- **T0144 is not complete** until Android lint, APK build, and Android JVM tests pass on the same commit.
