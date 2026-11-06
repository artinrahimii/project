#!/usr/bin/env python3
"""Generate fail-closed model-license notices from the checked-in sample catalog."""
from __future__ import annotations
import argparse
import hashlib
import json
import re
import shutil
import sys
from pathlib import Path
from urllib.parse import urlparse

APACHE_LICENSE_URL = "https://www.apache.org/licenses/LICENSE-2.0.txt"
APACHE_MARKERS = ("Apache License", "Version 2.0", "END OF TERMS AND CONDITIONS")
MODEL_ID_PATTERN = re.compile(r"^[a-z0-9][a-z0-9._-]{0,127}$")
SHA256_PATTERN = re.compile(r"^[A-Fa-f0-9]{64}$")


def require_https(value: object, field: str) -> str:
    if not isinstance(value, str) or not value.strip():
        raise ValueError(f"{field} must be a non-empty HTTPS URL")
    parsed = urlparse(value)
    if parsed.scheme != "https" or not parsed.hostname or parsed.username or parsed.password:
        raise ValueError(f"{field} must be a valid HTTPS URL")
    return value


def load_catalog(path: Path) -> tuple[dict, bytes]:
    raw = path.read_bytes()
    # Windows Git checkouts may convert committed LF bytes to CRLF. Normalize
    # only CRLF pairs so the catalog digest identifies the same Git text on
    # Windows and Linux, independent of core.autocrlf.
    canonical_raw = raw.replace(b"\r\n", b"\n")
    if b"\r" in canonical_raw:
        raise ValueError("Model catalog contains unsupported bare CR line endings")
    try:
        catalog = json.loads(canonical_raw.decode("utf-8"))
    except (UnicodeDecodeError, json.JSONDecodeError) as exc:
        raise ValueError(f"Model catalog is not valid UTF-8 JSON: {exc}") from exc
    if not isinstance(catalog, dict) or catalog.get("schemaVersion") != 1:
        raise ValueError("Model catalog must be a schemaVersion 1 object")
    models = catalog.get("models")
    if not isinstance(models, list) or not models:
        raise ValueError("Model catalog must contain at least one model")
    return catalog, canonical_raw


def validate_apache_license(path: Path) -> bytes:
    raw = path.read_bytes()
    if not 1024 <= len(raw) <= 128 * 1024:
        raise ValueError("Apache-2.0 license text has an unexpected byte length")
    try:
        text = raw.decode("utf-8")
    except UnicodeDecodeError as exc:
        raise ValueError("Apache-2.0 license text is not UTF-8") from exc
    missing = [marker for marker in APACHE_MARKERS if marker not in text]
    if missing:
        raise ValueError(f"Apache-2.0 license text is missing required markers: {missing}")
    return raw


def generate(catalog_path: Path, apache_license_path: Path, output_dir: Path) -> dict:
    catalog, catalog_raw = load_catalog(catalog_path)
    apache_raw = validate_apache_license(apache_license_path)
    catalog_hash = hashlib.sha256(catalog_raw).hexdigest()
    license_hash = hashlib.sha256(apache_raw).hexdigest()
    seen: set[str] = set()
    models_out: list[dict] = []
    for index, model in enumerate(catalog["models"]):
        if not isinstance(model, dict):
            raise ValueError(f"Model record {index} must be an object")
        model_id = model.get("id")
        if not isinstance(model_id, str) or not MODEL_ID_PATTERN.fullmatch(model_id):
            raise ValueError(f"Model record {index} has an invalid id")
        if model_id in seen:
            raise ValueError(f"Duplicate model id: {model_id}")
        seen.add(model_id)
        name = model.get("name")
        if not isinstance(name, str) or not name.strip():
            raise ValueError(f"Model {model_id} has no name")
        fmt = model.get("format")
        if not isinstance(fmt, str) or not fmt.strip():
            raise ValueError(f"Model {model_id} has no format")
        size = model.get("sizeBytes")
        if not isinstance(size, int) or isinstance(size, bool) or size <= 0:
            raise ValueError(f"Model {model_id} has an invalid declared size")
        checksum = model.get("checksum")
        if not isinstance(checksum, dict) or checksum.get("algorithm") != "sha256":
            raise ValueError(f"Model {model_id} must declare a SHA-256 checksum")
        model_hash = checksum.get("value")
        if not isinstance(model_hash, str) or not SHA256_PATTERN.fullmatch(model_hash):
            raise ValueError(f"Model {model_id} has an invalid SHA-256 checksum")
        source_url = require_https(model.get("sourceUrl"), f"{model_id}.sourceUrl")
        license_data = model.get("license")
        if not isinstance(license_data, dict) or license_data.get("status") != "verified":
            raise ValueError(f"Model {model_id} license status is not verified")
        identifier = license_data.get("identifier")
        if identifier != "Apache-2.0":
            raise ValueError(
                f"Model {model_id} uses unsupported license identifier {identifier!r}; "
                "add a separately reviewed license-text handler before packaging it"
            )
        license_url = require_https(license_data.get("url"), f"{model_id}.license.url")
        classification = license_data.get("classification", "unknown")
        if classification not in ("open_source", "restricted", "unknown"):
            raise ValueError(f"Model {model_id} has an invalid license classification")
        models_out.append({
            "id": model_id, "name": name, "format": fmt, "declared_size_bytes": size,
            "source_url": source_url, "expected_model_sha256": model_hash.lower(),
            "license": {
                "status": "verified", "identifier": identifier, "classification": classification,
                "model_card_url": license_url,
                "license_text_file": "files/WhiteP-Sample-Model-Apache-2.0.txt",
                "license_text_source_url": APACHE_LICENSE_URL,
                "license_text_sha256": license_hash,
            },
            "weights_included": False,
        })
    output_dir.mkdir(parents=True, exist_ok=True)
    files_dir = output_dir / "files"
    files_dir.mkdir(parents=True, exist_ok=True)
    license_relative = "files/WhiteP-Sample-Model-Apache-2.0.txt"
    shutil.copyfile(apache_license_path, output_dir / license_relative)
    manifest = {
        "schema_version": 1,
        "source_catalog": catalog_path.name,
        "source_catalog_sha256": catalog_hash,
        "source_catalog_hash_policy": "SHA-256 of UTF-8 bytes after CRLF-to-LF normalization; stable across Windows/Linux checkouts.",
        "license_text_source_url": APACHE_LICENSE_URL,
        "license_text_sha256": license_hash,
        "license_text_file": license_relative,
        "model_count": len(models_out),
        "models": models_out,
        "distribution_note": "Model weights are not included in this installer or release bundle.",
    }
    (output_dir / "model-licenses.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8", newline="\n",
    )
    lines = [
        "# White-P sample model license notice", "",
        "This notice is generated from the checked-in model catalog and the official Apache-2.0 license text.",
        "It records the model card's declared license metadata; it is not legal advice or an independent audit of every upstream right.",
        "**The model weights are not included** in this installer or release bundle. Downloading or redistributing model weights is a separate action.",
        "", f"- Catalog: {catalog_path.name}", f"- Catalog SHA-256 (UTF-8, CRLF normalized to LF): {catalog_hash}",
        f"- License text source: {APACHE_LICENSE_URL}", f"- Included license text: {license_relative}",
        f"- License text SHA-256: {license_hash}", "",
    ]
    for model in models_out:
        lic = model["license"]
        lines.extend([
            f"## {model['name']}", "", f"- Catalog ID: {model['id']}",
            f"- Format: {model['format']}", f"- Declared model file size: {model['declared_size_bytes']} bytes",
            f"- Model source: {model['source_url']}", f"- Expected model file SHA-256: {model['expected_model_sha256']}",
            f"- License status in catalog: {lic['status']}",
            f"- License identifier declared by model card: {lic['identifier']}",
            f"- Catalog classification: {lic['classification']} (not inferred when absent)",
            f"- Upstream model card: {lic['model_card_url']}",
            f"- Full license text: {lic['license_text_file']}",
            f"- License text SHA-256: {lic['license_text_sha256']}", "",
            "Review the upstream model card and the original base-model terms before any separate redistribution of model weights.", "",
        ])
    (output_dir / "ModelLicenses.md").write_text(
        "\n".join(lines).rstrip() + "\n", encoding="utf-8", newline="\n",
    )
    return manifest


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--catalog", required=True, type=Path)
    parser.add_argument("--apache-license-file", required=True, type=Path)
    parser.add_argument("--output-dir", required=True, type=Path)
    args = parser.parse_args()
    try:
        result = generate(args.catalog, args.apache_license_file, args.output_dir)
    except (OSError, ValueError) as exc:
        print(f"ERROR model-license-notice: {exc}", file=sys.stderr)
        return 2
    print(f"Model license notice: {result['model_count']} model(s); output: {args.output_dir}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
