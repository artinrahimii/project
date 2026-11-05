#!/usr/bin/env python3
"""Create a fail-closed third-party license inventory from NuGet restore assets."""
from __future__ import annotations
import argparse
import json
import re
import shutil
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

def local_name(tag: str) -> str:
    return tag.rsplit("}", 1)[-1]

def child_text(parent: ET.Element, name: str) -> str | None:
    for child in list(parent):
        if local_name(child.tag) == name and child.text and child.text.strip():
            return child.text.strip()
    return None

def safe_filename(value: str) -> str:
    return re.sub(r"[^A-Za-z0-9._-]+", "_", value).strip("._") or "license"

def resolve_package_license(package_id: str, version: str, package_root: Path, files_dir: Path) -> tuple[str, list[str]]:
    nuspecs = sorted(package_root.glob("*.nuspec"))
    if not nuspecs:
        raise ValueError(f"Missing nuspec for {package_id} {version}")
    root = ET.parse(nuspecs[0]).getroot()
    metadata = next((x for x in root.iter() if local_name(x.tag) == "metadata"), None)
    if metadata is None:
        raise ValueError(f"Missing nuspec metadata for {package_id} {version}")
    license_node = next((x for x in list(metadata) if local_name(x.tag) == "license"), None)
    license_url = child_text(metadata, "licenseUrl")
    copied: list[str] = []
    if license_node is not None and license_node.text and license_node.text.strip():
        value = license_node.text.strip()
        kind = license_node.attrib.get("type", "").lower()
        if kind == "expression":
            description = f"SPDX expression: {value}"
        elif kind == "file":
            relative = Path(value.replace("\\", "/"))
            if relative.is_absolute() or ".." in relative.parts:
                raise ValueError(f"Unsafe license path for {package_id} {version}")
            source = (package_root / relative).resolve()
            source.relative_to(package_root.resolve())
            if not source.is_file():
                raise ValueError(f"Declared license file is missing for {package_id} {version}")
            name = safe_filename(f"{package_id}-{version}-{source.name}")
            shutil.copyfile(source, files_dir / name)
            copied.append(f"files/{name}")
            description = f"License file: files/{name}"
        else:
            description = f"Package license metadata: {value} (type: {kind or 'unspecified'})"
    elif license_url:
        description = f"License URL: {license_url}"
    else:
        candidates = []
        for pattern in ("LICENSE", "LICENSE.*", "COPYING", "COPYING.*", "NOTICE", "NOTICE.*"):
            candidates.extend(x for x in package_root.glob(pattern) if x.is_file())
        if not candidates:
            raise ValueError(f"No license expression, URL, or packaged license file for {package_id} {version}")
        names = []
        for source in sorted(set(candidates)):
            name = safe_filename(f"{package_id}-{version}-{source.name}")
            shutil.copyfile(source, files_dir / name)
            copied.append(f"files/{name}")
            names.append(f"files/{name}")
        description = "Packaged license/notice files: " + ", ".join(names)
    if license_node is not None and license_node.attrib.get("type", "").lower() == "expression":
        for pattern in ("LICENSE", "LICENSE.*", "COPYING", "COPYING.*", "NOTICE", "NOTICE.*"):
            for source in sorted(package_root.glob(pattern)):
                if source.is_file():
                    name = safe_filename(f"{package_id}-{version}-{source.name}")
                    target = files_dir / name
                    if not target.exists():
                        shutil.copyfile(source, target)
                    item = f"files/{name}"
                    if item not in copied:
                        copied.append(item)
    return description, sorted(copied)

def generate(assets_file: Path, output_dir: Path) -> dict:
    assets = json.loads(assets_file.read_text(encoding="utf-8"))
    package_folders = [Path(x) for x in assets.get("packageFolders", {})]
    libraries = assets.get("libraries")
    if not package_folders or not isinstance(libraries, dict):
        raise ValueError("NuGet restore assets have no packageFolders/libraries")
    files_dir = output_dir / "files"
    files_dir.mkdir(parents=True, exist_ok=True)
    packages = []
    seen = set()
    for key, metadata in libraries.items():
        if not isinstance(metadata, dict) or metadata.get("type") != "package":
            continue
        if "/" not in key:
            raise ValueError(f"Invalid NuGet package key: {key}")
        package_id, version = key.rsplit("/", 1)
        identity = (package_id.casefold(), version.casefold())
        if identity in seen:
            continue
        seen.add(identity)
        relative = metadata.get("path") or f"{package_id.lower()}/{version.lower()}"
        package_root = next((folder / relative for folder in package_folders if (folder / relative).is_dir()), None)
        if package_root is None:
            raise ValueError(f"Restored package is missing: {package_id} {version}")
        license_text, copied = resolve_package_license(package_id, version, package_root, files_dir)
        packages.append({"id": package_id, "version": version, "license": license_text, "license_files": copied})
    packages.sort(key=lambda item: (item["id"].casefold(), item["version"].casefold()))
    output_dir.mkdir(parents=True, exist_ok=True)
    lines = [
        "# White-P third-party dependency notices", "",
        "Generated from the exact NuGet restore assets used to publish the Windows host.",
        "License identifiers and links are package metadata, not legal advice. Review redistribution terms.",
        "Notices shipped with the self-contained .NET runtime remain alongside the published runtime files.", "",
        f"Packages inventoried: {len(packages)}", ""
    ]
    for package in packages:
        lines.extend([f"## {package['id']} {package['version']}", "", f"- License: {package['license']}"])
        if package["license_files"]:
            lines.append("- Included files: " + ", ".join(package["license_files"]))
        lines.append("")
    (output_dir / "ThirdPartyNotices.md").write_text("\n".join(lines), encoding="utf-8", newline="\n")
    manifest = {"schema_version": 1, "source": assets_file.name, "package_count": len(packages), "packages": packages}
    (output_dir / "licenses.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="\n")
    return manifest

def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--assets", required=True, type=Path)
    parser.add_argument("--output-dir", required=True, type=Path)
    args = parser.parse_args()
    try:
        result = generate(args.assets, args.output_dir)
    except (OSError, ValueError, ET.ParseError, json.JSONDecodeError) as exc:
        print(f"ERROR dependency-license-inventory: {exc}", file=sys.stderr)
        return 2
    print(f"Dependency license inventory: {result['package_count']} packages; output: {args.output_dir}")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
