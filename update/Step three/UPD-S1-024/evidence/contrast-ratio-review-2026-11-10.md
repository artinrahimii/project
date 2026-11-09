# Shared palette contrast review — 2026-11-10

## Method

Ratios were calculated from the WCAG relative-luminance formula for the exact five approved hexadecimal tokens in `assets/brand/whitep-design-tokens.json`. This is a source-level palette pairing check; it does not replace reviewing every rendered control and screen.

## Measured pairings

- `midnight_ink` / `porcelain`: **13.691:1** — passes normal text (4.5:1) and essential component (3:1) thresholds.
- `midnight_ink` / `pure_white`: **15.561:1** — passes.
- `pure_white` / `midnight_ink`: **15.561:1** — passes; this is the WPF primary button text/surface pairing.
- `pure_white` / `muted_teal`: **4.556:1** — passes normal-text threshold, where this pairing is used.
- `muted_teal` / `porcelain`: **4.008:1** — above 3:1 for essential non-text borders/controls, but below 4.5:1 for normal text. Do not use it as normal text on porcelain. The Android theme uses midnight ink for primary and secondary text; WPF uses teal for control borders rather than body text.
- `antique_gold` / `porcelain`: **2.358:1**.
- `antique_gold` / `pure_white`: **2.680:1**. Gold remains a decorative accent/rim; it is not used as small text or the sole essential status indicator.

## Actual rendered evidence

The WPF screenshot in Product Build run [42704515813](https://github.com/artinrahimii/project/actions/runs/42704515813), artifact `13713870555`, shows the Connect, Start local engine and Estimate compatibility labels visibly rendered white on navy after the canonical generator emitted the Button ContentTemplate.

## Remaining review

- Android Home, Conversations, conversation detail and Messages views must still be captured and inspected on the clean Android build.
- The Windows smoke screenshot is a runner view and does not prove a user-installed production host or live model readiness.
