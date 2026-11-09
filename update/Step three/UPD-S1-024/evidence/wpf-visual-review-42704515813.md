# WPF visual review — 2026-11-10

- Source commit for the artifact: `V 3.1.131`, SHA `725a1f59f690b819d7d8ce19b2d4861227cb9527`.
- Product Build: [run 42704515813](https://github.com/artinrahimii/project/actions/runs/42704515813).
- Artifact: `whitep-brand-review-windows`, ID `13713870555`.
- Captured file: `wpf-main-window.png`, 900 × 850 pixels.
- Actual WPF startup screenshot now shows the previously blank primary action labels: Connect, Start local engine, Estimate compatibility. They are white on the navy button surface.
- The same window reports that host capacity is unknown/not ready, the local engine is not started, the backend is stopped, and direct peer-to-peer transport is not implemented in this build. The screenshot proves the visual labels in the runner smoke window only; it does not prove model readiness or a complete host.
- The matching generated ICO has seven embedded sizes: 16, 24, 32, 48, 64, 128, 256 pixels.
