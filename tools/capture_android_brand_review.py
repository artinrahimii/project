#!/usr/bin/env python3
"""Capture real Android emulator screens for White-P visual identity review."""
from __future__ import annotations

import re
import subprocess
import time
import xml.etree.ElementTree as ET
from pathlib import Path


def parse_bounds(value: str) -> tuple[int, int]:
    match = re.fullmatch(r"\[(\d+),(\d+)\]\[(\d+),(\d+)\]", value)
    if match is None:
        raise ValueError(f"Unsupported Android UI bounds: {value!r}")
    left, top, right, bottom = (int(part) for part in match.groups())
    return (left + right) // 2, (top + bottom) // 2


def run_adb(*args: str, capture: bool = False) -> bytes:
    result = subprocess.run(
        ["adb", *args],
        check=True,
        stdout=subprocess.PIPE if capture else subprocess.DEVNULL,
        stderr=subprocess.PIPE,
    )
    return result.stdout if capture else b""


def current_ui_tree() -> ET.Element:
    run_adb("shell", "uiautomator", "dump", "/sdcard/whitep-brand-review.xml")
    data = run_adb("exec-out", "cat", "/sdcard/whitep-brand-review.xml", capture=True)
    return ET.fromstring(data)


def find_text_center(root: ET.Element, label: str) -> tuple[int, int]:
    nodes = [
        node for node in root.iter("node")
        if node.attrib.get("text", "").strip() == label
        or node.attrib.get("content-desc", "").strip() == label
    ]
    nodes.sort(key=lambda node: node.attrib.get("clickable") != "true")
    for node in nodes:
        bounds = node.attrib.get("bounds")
        if bounds:
            return parse_bounds(bounds)
    raise RuntimeError(f"Android UI did not expose a visible node labelled {label!r}")


def tap_label(label: str) -> None:
    x, y = find_text_center(current_ui_tree(), label)
    run_adb("shell", "input", "tap", str(x), str(y))
    time.sleep(1.0)


def capture(directory: Path, filename: str) -> None:
    data = run_adb("exec-out", "screencap", "-p", capture=True)
    if not data.startswith(b"\x89PNG\r\n\x1a\n") or len(data) < 1024:
        raise RuntimeError(f"Screenshot {filename} is not a valid PNG")
    (directory / filename).write_bytes(data)
    print(f"Captured {filename} ({len(data)} bytes)")


def main() -> int:
    output = Path("artifacts/brand-review/android")
    output.mkdir(parents=True, exist_ok=True)
    time.sleep(1.0)
    capture(output, "android-home.png")

    tap_label("Conversations")
    capture(output, "android-conversations.png")
    tap_label("New conversation")
    capture(output, "android-conversation-detail.png")
    tap_label("Back to conversations")

    tap_label("Connections")
    capture(output, "android-connections.png")
    tap_label("Privacy")
    capture(output, "android-privacy.png")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
