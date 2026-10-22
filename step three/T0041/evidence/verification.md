# T0041 verification evidence

## Files inspected and updated
- `src/Host.Windows/App.xaml`: the real WPF StartupUri points to `MainWindow.xaml`.
- `src/Host.Windows/App.xaml.cs`: now supports `--smoke-test` through the same startup path.
- `src/Host.Windows/MainWindow.xaml`: required controls are named in the real main-window XAML.
- `src/Host.Windows/MainWindow.xaml.cs`: constructor initializes components/status and window close disposes the host session.
- `.github/workflows/product-build.yml`: Windows CI runs the compiled `WhiteP.Host.Windows.exe --smoke-test` with a 15-second timeout.
- `tests/test_wpf_entrypoint.py`: source contract guards for startup and smoke path.

## Runtime acceptance to be proven by CI
A pass requires the actual built WPF executable to start through `StartupUri`, show a loaded/visible main window, find server/model/connect/status/prompt/output/generate controls, and exit 0 after clean close. The smoke process is terminated and the job fails if it exceeds 15 seconds. Static guards do not substitute for this process-level check; exact-commit CI is pending.
