# Automated palette contrast regression — 2026-11-10

The new unit test computes contrast from the canonical token JSON using the WCAG relative-luminance formula and protects the following measured pairings:

| Foreground / background | Contrast | Required use |
|---|---:|---|
| midnight_ink / porcelain | 13.691:1 | Normal text |
| midnight_ink / pure_white | 15.561:1 | Normal text |
| pure_white / midnight_ink | 15.561:1 | Primary button text |
| pure_white / muted_teal | 4.556:1 | Normal text when paired |
| muted_teal / porcelain | 4.008:1 | Borders/non-text emphasis only; not normal text |
| antique_gold / porcelain | 2.358:1 | Decorative only |
| antique_gold / pure_white | 2.680:1 | Decorative only |
| midnight_ink / antique_gold | 5.807:1 | High-contrast ink-on-gold pairing |

The regression also checks that Android theme text roles remain `midnight_ink`, `muted_teal` stays the secondary action color with `pure_white` foreground, and the WPF generated style uses navy/white buttons and teal borders. The next quality run must pass before this evidence is considered CI-verified.
