# Android release signing

## Security boundary

The Android release keystore and its passwords must never be committed, placed in the working tree, printed to CI logs, or stored in repository files. The Gradle release variant reads four environment variables and rejects release-related tasks when any input is missing. It also rejects any keystore path that does not resolve outside the repository checkout.

The normal Product Build creates a fresh disposable JKS key under the GitHub runner's temporary directory, builds `assembleRelease`, verifies the APK signature with `apksigner`, then deletes the temporary key. This proves the Gradle signing path works; the disposable certificate is **not** a production identity and must never be used to distribute the app or sign future updates.

## Configure the protected GitHub Environment

Create a GitHub Actions Environment named `android-release`. Configure required reviewers and deployment-ref restrictions before adding the following Environment secrets:

- `ANDROID_RELEASE_KEYSTORE_BASE64`: Base64-encoded bytes of the production JKS keystore.
- `ANDROID_RELEASE_STORE_PASSWORD`: keystore password.
- `ANDROID_RELEASE_KEY_ALIAS`: alias of the release key entry.
- `ANDROID_RELEASE_KEY_PASSWORD`: password for that key entry.

Add this Environment variable (not a secret):

- `ANDROID_RELEASE_CERT_SHA256`: the SHA-256 fingerprint of the exact production signing certificate, as 64 hexadecimal characters. Colons and whitespace are accepted and normalized by the workflow.

Do not assume that Base64 is encryption. Treat the encoded keystore as the original private key: restrict who can read and change the Environment, use a trusted device to prepare it, and keep an offline encrypted backup and recovery details under the maintainer's control.

## Prepare and provision the production key

Generate or import the release keystore on a trusted machine that is not the repository checkout. Use a durable backup before using it for distribution. For a new JKS key, `keytool -genkeypair` can prompt for passwords interactively; do not put passwords directly into shell history or a checked-in script. Keep the same signing identity for all updates to the same Android application.

Obtain the SHA-256 certificate fingerprint from the selected alias using `keytool -list -v -keystore <path> -storetype JKS -alias <alias>`. Copy only the `SHA256` certificate fingerprint into `ANDROID_RELEASE_CERT_SHA256`, normalized to 64 hexadecimal characters.

Encode the keystore file as Base64 on the trusted machine and paste the result directly into the protected Environment secret `ANDROID_RELEASE_KEYSTORE_BASE64`. On Windows PowerShell, `[Convert]::ToBase64String([IO.File]::ReadAllBytes('<path>'))` produces the encoded value; do not write that value to a repository file. Base64 does not protect the key if the encoded value is exposed.

## Run the signed release workflow

The `Android signed release` workflow can be started manually or by pushing a tag matching `android-v*`. It uses the protected `android-release` Environment, decodes the production keystore only into the runner's temporary directory, runs a clean `assembleRelease`, verifies the APK with `apksigner`, compares the actual certificate fingerprint to `ANDROID_RELEASE_CERT_SHA256`, uploads `whitep-android-release-signed`, and removes the temporary keystore in an always-run cleanup step.

If any required secret is missing, the keystore cannot be read, release signing inputs point inside the checkout, the APK signature is invalid, or the signer fingerprint differs from the pinned value, the release job fails closed.

This task produces a signed APK artifact only. It does not create a public GitHub Release or a checksum file. Checksum generation and release publication belong to the subsequent release tasks.

## What has and has not been verified

- The regular Product Build exercises the release signing configuration with a new disposable key and verifies that an APK is signed.
- Production signing is not considered ready until the maintainer provisions the four Environment secrets and the pinned certificate fingerprint, then runs the protected signed-release workflow successfully.
- The GitHub connector used during this implementation could not inspect Environment secret values or confirm their provisioning. No production keystore or password was generated, requested, or committed as part of this change.
