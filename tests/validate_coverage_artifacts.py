#!/usr/bin/env python3
"""Validate real CI coverage and critical-test artifacts without imposing an arbitrary coverage threshold."""

from __future__ import annotations

import argparse
import json
import xml.etree.ElementTree as ET
from pathlib import Path
from typing import Any


def parse_cobertura(path: Path) -> dict[str, Any]:
    root = ET.parse(path).getroot()
    if root.tag.split("}")[-1] != "coverage":
        raise ValueError(f"{path} is not a Cobertura coverage report")
    lines_valid = int(root.attrib.get("lines-valid", "0"))
    lines_covered = int(root.attrib.get("lines-covered", "0"))
    if lines_valid <= 0 or not 0 <= lines_covered <= lines_valid:
        raise ValueError(f"{path} has invalid or empty line coverage counters")
    rate = float(root.attrib.get("line-rate", lines_covered / lines_valid))
    if not 0.0 <= rate <= 1.0:
        raise ValueError(f"{path} has an invalid line-rate")
    return {
        "path": path.as_posix(),
        "lines_valid": lines_valid,
        "lines_covered": lines_covered,
        "line_rate": rate,
        "line_coverage_percent": round(rate * 100.0, 2),
    }


def validate_html_report(path: Path) -> dict[str, Any]:
    data = path.read_bytes()
    text = data.decode("utf-8", errors="replace").lower()
    if len(data) < 100 or "<html" not in text or "</html>" not in text:
        raise ValueError(f"{path} is not a complete, non-empty HTML coverage report")
    return {"path": path.as_posix(), "size_bytes": len(data)}


def parse_junit_directories(directories: list[Path]) -> dict[str, int]:
    xml_files: list[Path] = []
    for directory in directories:
        if not directory.is_dir():
            raise FileNotFoundError(f"JUnit result directory does not exist: {directory}")
        xml_files.extend(sorted(directory.rglob("*.xml")))
    tests = failures = errors = skipped = 0
    parsed_files = 0
    for path in xml_files:
        root = ET.parse(path).getroot()
        cases = [
            item for item in root.iter()
            if item.tag.split("}")[-1] == "testcase"
        ]
        if not cases:
            continue
        parsed_files += 1
        tests += len(cases)
        for case in cases:
            children = {child.tag.split("}")[-1] for child in case}
            failures += int("failure" in children)
            errors += int("error" in children)
            skipped += int("skipped" in children)
    if parsed_files == 0 or tests == 0:
        raise ValueError("No JUnit test cases were found in the supplied result directories")
    if failures or errors:
        raise ValueError(f"Critical test result reports contain {failures} failures and {errors} errors")
    return {
        "report_files": parsed_files,
        "tests": tests,
        "failures": failures,
        "errors": errors,
        "skipped": skipped,
        "passed": tests - failures - errors - skipped,
    }


def parse_critical_log(path: Path) -> dict[str, int]:
    text = path.read_text(encoding="utf-8", errors="replace")
    if not text.strip():
        raise ValueError(f"Critical test log is empty: {path}")
    pass_markers = sum("PASS:" in line for line in text.splitlines())
    skip_markers = sum("SKIP:" in line for line in text.splitlines())
    if pass_markers == 0:
        raise ValueError(f"No explicit PASS markers were found in {path}")
    return {
        "path": path.as_posix(),
        "pass_markers": pass_markers,
        "skip_markers": skip_markers,
        "size_bytes": path.stat().st_size,
    }


def build_summary(args: argparse.Namespace) -> dict[str, Any]:
    summary: dict[str, Any] = {"schema_version": 1}
    if args.cobertura:
        if not args.critical_log:
            raise ValueError("--critical-log is required with --cobertura")
        summary["coverage"] = {"format": "cobertura", **parse_cobertura(args.cobertura)}
        summary["critical_test_log"] = parse_critical_log(args.critical_log)
    if args.html_report:
        if not args.junit_dir:
            raise ValueError("--junit-dir is required with --html-report")
        summary["coverage_reports"] = [validate_html_report(p) for p in args.html_report]
        summary["junit_results"] = parse_junit_directories(args.junit_dir)
    if not args.cobertura and not args.html_report:
        raise ValueError("Provide --cobertura or at least one --html-report")
    return summary


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--cobertura", type=Path)
    parser.add_argument("--critical-log", type=Path)
    parser.add_argument("--html-report", type=Path, action="append", default=[])
    parser.add_argument("--junit-dir", type=Path, action="append", default=[])
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    summary = build_summary(args)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(summary, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(summary, ensure_ascii=False, sort_keys=True))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
