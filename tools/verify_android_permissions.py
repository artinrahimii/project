#!/usr/bin/env python3
"""Audit source and packaged Android permission declarations against a minimal allowlist."""
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
DYNAMIC_RECEIVER_SUFFIX = ".DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION"
AAPT_PERMISSION_LINE = re.compile(
    r"""^\s*uses-permission(?:-sdk-[A-Za-z0-9_-]+)?\s*:\s*name=['"]([^'"]+)['"]""",
    re.MULTILINE,
)
XMLTREE_ELEMENT = re.compile(r"^(\s*)E:\s+permission(?:\s|\(|$)")
XMLTREE_ANDROID_ATTRIBUTE_PREFIX = r"(?:android:|http://schemas\.android\.com/apk/res/android:)"
XMLTREE_NAME = re.compile(
    rf'A:\s+{XMLTREE_ANDROID_ATTRIBUTE_PREFIX}name\([^)]+\)="([^"]+)"'
)
XMLTREE_PROTECTION_LEVEL = re.compile(
    rf"A:\s+{XMLTREE_ANDROID_ATTRIBUTE_PREFIX}protectionLevel\([^)]+\)="
    r"(?:\(type\s+[^)]+\))?(0x[0-9a-fA-F]+)"
)


class PermissionPolicyError(ValueError):
    """Raised when the app requests a permission outside its reviewed allowlist."""


def read_manifest_permissions(manifest_path: Path) -> list[str]:
    """Read source-requested permissions, including SDK-qualified elements."""
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
    """Extract requested permissions from aapt2 dump permissions."""
    return AAPT_PERMISSION_LINE.findall(output)


def parse_declared_permissions(xmltree_output: str) -> list[dict[str, int | str | None]]:
    """Extract app-declared permission names and numeric protection levels from AAPT2 XML tree."""
    if not re.search(r"^\s*E:\s+manifest(?:\s|\(|$)", xmltree_output, re.MULTILINE):
        raise PermissionPolicyError("aapt2 xmltree output does not contain a manifest root.")

    lines = xmltree_output.splitlines()
    declarations: list[dict[str, int | str | None]] = []
    index = 0
    while index < len(lines):
        element_match = XMLTREE_ELEMENT.match(lines[index])
        if not element_match:
            index += 1
            continue

        indent = len(element_match.group(1))
        name: str | None = None
        protection_level: int | None = None
        index += 1
        while index < len(lines):
            line = lines[index]
            child_element = re.match(r"^(\s*)E:\s+", line)
            if child_element and len(child_element.group(1)) <= indent:
                break

            name_match = XMLTREE_NAME.search(line)
            if name_match:
                name = name_match.group(1)
            level_match = XMLTREE_PROTECTION_LEVEL.search(line)
            if level_match:
                protection_level = int(level_match.group(1), 16)
            index += 1

        declarations.append({"name": name, "protection_level": protection_level})

    return declarations


def validate_permission_policy(
    source_permissions: list[str],
    packaged_permissions: list[str],
    declared_permissions: list[dict[str, int | str | None]],
    application_id: str,
) -> None:
    """Allow INTERNET plus AndroidX's exact app-scoped signature receiver permission."""
    if not re.fullmatch(r"[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)+", application_id):
        raise PermissionPolicyError(f"Invalid application ID: {application_id!r}.")

    if source_permissions != [EXPECTED_PERMISSION]:
        raise PermissionPolicyError(
            "Source AndroidManifest.xml must request exactly "
            f"{EXPECTED_PERMISSION!r}; found {source_permissions!r}."
        )

    private_receiver_permission = application_id + DYNAMIC_RECEIVER_SUFFIX
    expected_permissions = {EXPECTED_PERMISSION, private_receiver_permission}
    packaged_set = set(packaged_permissions)
    unexpected = sorted(packaged_set - expected_permissions)
    missing = sorted(expected_permissions - packaged_set)
    if unexpected or missing or len(packaged_permissions) != len(expected_permissions):
        details = []
        if unexpected:
            details.append(f"unexpected packaged permissions: {unexpected!r}")
        if missing:
            details.append(f"required packaged permissions missing: {missing!r}")
        if len(packaged_permissions) != len(expected_permissions):
            details.append("packaged permission output contains duplicate or unexpected entries")
        raise PermissionPolicyError("; ".join(details))

    declarations = [
        item for item in declared_permissions
        if item.get("name") == private_receiver_permission
    ]
    if len(declarations) != 1:
        parsed = [
            (item.get("name"), item.get("protection_level"))
            for item in declared_permissions
        ]
        raise PermissionPolicyError(
            "The exact AndroidX dynamic-receiver permission must be declared exactly once "
            "in the packaged manifest; AAPT2 parsed declarations: "
            f"{parsed!r}."
        )
    level = declarations[0].get("protection_level")
    if level != 0x2:
        raise PermissionPolicyError(
            "The AndroidX dynamic-receiver permission must use protectionLevel=signature "
            f"(0x2); found {level!r}."
        )

    other_declarations = [
        item.get("name") for item in declared_permissions
        if item.get("name") != private_receiver_permission
    ]
    if other_declarations:
        raise PermissionPolicyError(
            f"Unexpected custom permission declarations in packaged manifest: {other_declarations!r}."
        )


def _run_aapt2(aapt2_path: Path, *arguments: str) -> str:
    result = subprocess.run(
        [str(aapt2_path), *arguments],
        check=False,
        capture_output=True,
        text=True,
        encoding="utf-8",
    )
    if result.returncode != 0:
        raise RuntimeError(
            f"aapt2 {' '.join(arguments[:2])} failed "
            f"(exit {result.returncode}): {result.stderr.strip()}"
        )
    return result.stdout


def verify_apk_permissions(
    apk_path: Path,
    aapt2_path: Path,
    manifest_path: Path = DEFAULT_MANIFEST,
    application_id: str = "com.whitep.client",
) -> list[str]:
    """Verify source manifest and effective permission/security declarations in the APK."""
    if not apk_path.is_file():
        raise FileNotFoundError(f"Android APK not found: {apk_path}")
    if not aapt2_path.is_file():
        raise FileNotFoundError(f"Android SDK aapt2 executable not found: {aapt2_path}")

    source_permissions = read_manifest_permissions(manifest_path)
    packaged_permissions = parse_aapt_permissions(
        _run_aapt2(aapt2_path, "dump", "permissions", str(apk_path))
    )
    if not packaged_permissions:
        raise PermissionPolicyError(
            "aapt2 returned no uses-permission entries; the APK permission set could not be verified."
        )

    xmltree_output = _run_aapt2(
        aapt2_path, "dump", "xmltree", "--file", "AndroidManifest.xml", str(apk_path)
    )
    declarations = parse_declared_permissions(xmltree_output)
    validate_permission_policy(
        source_permissions, packaged_permissions, declarations, application_id
    )
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
    parser.add_argument(
        "--application-id",
        default="com.whitep.client",
        help="Expected Android application ID used to scope AndroidX's private receiver permission.",
    )
    args = parser.parse_args(argv)

    try:
        permissions = verify_apk_permissions(
            args.apk, args.aapt2, args.manifest, args.application_id
        )
    except (OSError, UnicodeError, ET.ParseError, PermissionPolicyError, RuntimeError) as exc:
        print(f"ERROR ANDROID_PERMISSION_POLICY: {exc}", file=sys.stderr)
        return 1

    print("Android permission policy passed.")
    print(f"Packaged permissions: {', '.join(sorted(permissions))}")
    print(
        "Verified AndroidX private receiver permission: "
        f"{args.application_id}{DYNAMIC_RECEIVER_SUFFIX} (signature)"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
