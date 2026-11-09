# White-P shared visual identity

Canonical sources are `assets/brand/whitep-coin.svg` and `assets/brand/whitep-design-tokens.json`. The SVG is the only icon master; the token JSON is the sole source for the approved five colors and semantic roles.

Generate platform outputs before a local build with `python tools/generate_brand_assets.py`. The standard-library generator creates the Windows multi-resolution ICO, Android density icons and adaptive foregrounds, and the platform color resources. Generated files must not be edited manually.

Both platform jobs in the product-build workflow and the production-signed Android release workflow generate these assets from a clean checkout. The Python quality suite renders the icon into a temporary directory and verifies the ICO, PNG sizes, adaptive icon references, and color resources.

Shared roles: porcelain page background; pure-white elevated surfaces; midnight-ink text and primary action; muted-teal secondary action and component boundary; antique-gold decorative accent. Muted teal is not normal-size text on porcelain. Antique gold is not small text or the sole essential state indicator.
