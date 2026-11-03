#!/usr/bin/env python3
"""Verify that the packaged Android app requests only the reviewed network permission."""
from __future__ import annotations

import argparse
import re
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DEFAULT_MANIFEST = ROOT / "src/Client.Android/app/src/main/AndroidManifest.xml"
EXPECTED_PERMISSION = "android.permission.INTERNET"
AAPT_PERMISSION_LINE = re.compile(
    r"""^\s*uses-permission(?:-sdk-[A-Za-z0-9_-]+)?\s*:\s*name=['"]([^'"]+)['"]""",
    re.MULTILINE,
)


class PermissionPolicyError(ValueError):
    """Raised when the app requests a permission outside its reviewed allowlist."""


def read_manifest_permissions(manifest_path: Path) -> list[str]:
    """Read requested permissions, including SDK-qualified uses-permission elements."""
    root = ET.parse(manifest_path).getroot()
    permissions: list[str] = []
    for element in root.iter():
        tag = element.tag.rsplit("}", 1)[-1]
        if tag == "uses-permission" or tag.startswith("uses-permission-sdk-"):
            name = element.attrib.get("{http://schemas.android.com/apk/res/android}name")
            if not name:
                raise PermissionPolicyError(f"{tag} is missing android:name.")
            permissions.append(name)
    return permissions


def parse_aapt_permissions(output: str) -> list[str]:
    """Extract effective uses-permission entries from aapt2 dump permissions output."""
    return AAPT_PERMISSION_LINE.findall(output)


def validate_permission_policy(
    source_permissions: list[str],
    packaged_permissions: list[str],
) -> None:
    """Require INTERNET only in both the source manifest and packaged APK."""
    expected = [EXPECTED_PERMISSION]
    if source_permissions != expected:
        raise PermissionPolicyError(
            "Source AndroidManifest.xml must request exactly "
            f"{EXPECTED_PERMISSION!r}; found {source_permissions!r}."
        )

    packaged = set(packaged_permissions)
    expected_set = {EXPECTED_PERMISSION}
    unexpected = sorted(packaged - expected_set)
    missing = sorted(expected_set - packaged)
    if unexpected or missing:
        details = []
        if unexpected:
            details.append(f"unexpected packaged permissions: {unexpected!r}")
        if missing:
            details.append(f"required packaged permissions missing: {missing!r}")
        raise PermissionPolicyError("; ".join(details))


def verify_apk_permissions(
    apk_path: Path,
    aapt2_path: Path,
    manifest_path: Path = DEFAULT_MANIFEST,
) -> list[str]:
    """Verify source and merged APK permissions using the Android SDK's aapt2 tool."""
    if not apk_path.is_file():
        raise FileNotFoundError(f"Android APK not found: {apk_path}")
    if not aapt2_path.is_file():
        raise FileNotFoundError(f"Android SDK aapt2 executable not found: {aapt2_path}")

    source_permissions = read_manifest_permissions(manifest_path)
    result = subprocess.run(
        [str(aapt2_path), "dump", "permissions", str(apk_path)],
        check=False,
        capture_output=True,
        text=True,
        encoding="utf-8",
    )
    if result.returncode != 0:
        raise RuntimeError(
            "aapt2 dump permissions failed "
            f"(exit {result.returncode}): {result.stderr.strip()}"
        )

    packaged_permissions = parse_aapt_permissions(result.stdout)
    if not packaged_permissions:
        raise PermissionPolicyError(
            "aapt2 returned no uses-permission entries; the APK permission set "
            "could not be verified."
        )
    validate_permission_policy(source_permissions, packaged_permissions)
    return packaged_permissions


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apk", required=True, type=Path, help="Packaged Android APK to inspect.")
    parser.add_argument("--aapt2", required=True, type=Path, help="Android SDK aapt2 executable.")
    parser.add_argument(
        "--manifest",
        type=Path,
        default=DEFAULT_MANIFEST,
        help="Source Android manifest (defaults to the app's main manifest).",
    )
    args = parser.parse_args(argv)

    try:
        permissions = verify_apk_permissions(args.apk, args.aapt2, args.manifest)
    except (OSError, UnicodeError, ET.ParseError, PermissionPolicyError, RuntimeError) as exc:
        print(f"ERROR ANDROID_PERMISSION_POLICY: {exc}", file=sys.stderr)
        return 1

    print("Android permission policy passed.")
    print(f"Packaged permissions: {', '.join(permissions)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
