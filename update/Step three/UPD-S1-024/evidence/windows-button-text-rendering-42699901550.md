# WPF button-label visibility defect — 2026-11-10

## Source evidence

- Product Build run: [42703515290](https://github.com/artinrahimii/project/actions/runs/42703515290).
- Visual artifact: `whitep-brand-review-windows`, artifact ID `13713470695`, captured from the actual WPF startup smoke window on `V 3.1.128`.
- Screenshot observation: the dark navy button surfaces for Connect, Start local engine and Estimate compatibility had no visible labels. This fails visual readability even though the palette tokens and Button foreground resource were set to the intended colors.

## Correction

The global WPF Button style now supplies a `ContentTemplate` containing a `TextBlock` with an explicit `PureWhiteBrush` foreground. This avoids relying on the default string-content presenter inheriting the Button foreground through the implicit TextBlock style.

A regression test verifies the content template and white foreground declaration. Actual rendering is rechecked from the next clean WPF build screenshot; the source assertion alone is not visual acceptance.
