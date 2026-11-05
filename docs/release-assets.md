# White-P release assets

## Files

The release workflow publishes these files from successful build artifacts associated with the exact same Git commit:

- WhiteP-Android-0.1.0.apk: production-signed Android package. The signed-release workflow must have verified its certificate fingerprint against the protected android-release Environment variable before uploading it.
- WhiteP-Windows-Installer.zip: Windows x64 installer executable plus the generated third-party license inventory and notices.
- SHA256SUMS.txt: SHA-256 checksums for both platform assets and the provenance file.
- BUILD-PROVENANCE.txt: source commit and the two successful GitHub Actions run IDs used to create this release bundle.

Verify the downloaded files from the directory containing the assets:

~~~bash
sha256sum --check SHA256SUMS.txt
~~~

On Windows, use Get-FileHash -Algorithm SHA256 and compare its output with the corresponding entry in SHA256SUMS.txt.

## Create a draft release

1. Update the main branch and run the protected Android signing workflow for that exact commit:

   ~~~cmd
   gh workflow run 408305382 --repo artinrahimii/project --ref main
   ~~~

2. Confirm that the run succeeds and uploads whitep-android-release-signed. Confirm that the White-P product build on the same commit succeeds and uploads whitep-windows-installer.
3. From a clean local checkout of that same main commit, create and push an android-v* tag. For the first 0.1.0 candidate:

   ~~~cmd
   git fetch origin main
   git switch main
   git pull --ff-only origin main
   git tag -a android-v0.1.0 -m "White-P 0.1.0"
   git push origin android-v0.1.0
   ~~~

4. The White-P release assets workflow finds successful signed Android and product-build runs for the exact tagged commit, creates and verifies SHA256SUMS.txt, and creates or updates a draft GitHub Release with the assets.

Do not move the tag to a different commit to work around a failed or missing build. Create a new version tag when the source commit changes. If a release with that tag is already published, the workflow refuses to overwrite it.

## Release status

A draft release is a candidate distribution bundle, not a declaration that the complete product has passed final acceptance. Keep it as a draft until the remaining end-to-end and release-readiness tasks, including T0218, have passed. The workflow does not publish a public release automatically.
