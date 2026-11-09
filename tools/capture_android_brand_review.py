#!/usr/bin/env python3
"""Capture real Android emulator screens for White-P visual identity review."""
from __future__ import annotations

import re
import shutil
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


def ui_tree_has_text(root: ET.Element, label: str) -> bool:
    return any(
        node.attrib.get("text", "").strip() == label
        or node.attrib.get("content-desc", "").strip() == label
        for node in root.iter("node")
    )


def find_text_center(root: ET.Element, label: str) -> tuple[int, int]:
    parents = {
        child: parent
        for parent in root.iter()
        for child in parent
    }
    nodes = [
        node for node in root.iter("node")
        if node.attrib.get("text", "").strip() == label
        or node.attrib.get("content-desc", "").strip() == label
    ]
    for node in nodes:
        target = node
        while target is not None:
            if target.attrib.get("clickable") == "true":
                bounds = target.attrib.get("bounds")
                if bounds:
                    return parse_bounds(bounds)
            target = parents.get(target)
    for node in nodes:
        bounds = node.attrib.get("bounds")
        if bounds:
            return parse_bounds(bounds)
    raise RuntimeError(f"Android UI did not expose a visible node labelled {label!r}")


def ui_tree_has_text_prefix(root: ET.Element, prefix: str) -> bool:
    return any(
        node.attrib.get("text", "").strip().startswith(prefix)
        or node.attrib.get("content-desc", "").strip().startswith(prefix)
        for node in root.iter("node")
    )


def wait_for_text(label: str, timeout_seconds: float = 20.0) -> ET.Element:
    deadline = time.monotonic() + timeout_seconds
    while True:
        root = current_ui_tree()
        if ui_tree_has_text(root, label):
            return root
        if time.monotonic() >= deadline:
            raise RuntimeError(
                f"Android did not reach expected UI text {label!r} within "
                f"{timeout_seconds:.1f} seconds; visible node count="
                f"{sum(1 for _ in root.iter('node'))}"
            )
        time.sleep(0.5)


def wait_for_text_prefix(prefix: str, timeout_seconds: float = 20.0) -> ET.Element:
    deadline = time.monotonic() + timeout_seconds
    while True:
        root = current_ui_tree()
        if ui_tree_has_text_prefix(root, prefix):
            return root
        if time.monotonic() >= deadline:
            raise RuntimeError(
                f"Android did not reach expected UI text prefix {prefix!r} within "
                f"{timeout_seconds:.1f} seconds; visible node count="
                f"{sum(1 for _ in root.iter('node'))}"
            )
        time.sleep(0.5)


def scroll_to_text(
    label: str,
    *,
    direction: str = "up",
    max_swipes: int = 6,
) -> ET.Element:
    if direction not in {"up", "down"}:
        raise ValueError("Scroll direction must be 'up' or 'down'.")
    for attempt in range(max_swipes + 1):
        root = current_ui_tree()
        if ui_tree_has_text(root, label):
            print(f"Located {label!r} after {attempt} scroll gesture(s).")
            return root
        if attempt == max_swipes:
            break
        if direction == "up":
            run_adb("shell", "input", "swipe", "540", "2100", "540", "650", "450")
        else:
            run_adb("shell", "input", "swipe", "540", "650", "540", "2100", "450")
        time.sleep(0.35)
    root = current_ui_tree()
    raise RuntimeError(
        f"Android could not bring expected UI text {label!r} into view after "
        f"{max_swipes} {direction} scroll gesture(s); visible node count="
        f"{sum(1 for _ in root.iter('node'))}"
    )


def tap_label(label: str) -> None:
    x, y = find_text_center(current_ui_tree(), label)
    run_adb("shell", "input", "tap", str(x), str(y))
    time.sleep(0.25)


def capture(
    directory: Path,
    filename: str,
    required_label: str,
    *,
    required_prefix: str | None = None,
) -> None:
    root = current_ui_tree()
    if not ui_tree_has_text(root, required_label):
        raise RuntimeError(
            f"Refusing to capture {filename}: required visible landmark "
            f"{required_label!r} is absent"
        )
    if required_prefix is not None and not ui_tree_has_text_prefix(root, required_prefix):
        raise RuntimeError(
            f"Refusing to capture {filename}: required visible text prefix "
            f"{required_prefix!r} is absent"
        )
    data = run_adb("exec-out", "screencap", "-p", capture=True)
    if not data.startswith(b"\x89PNG\r\n\x1a\n") or len(data) < 1024:
        raise RuntimeError(f"Screenshot {filename} is not a valid PNG")
    (directory / filename).write_bytes(data)
    print(f"Captured {filename} after verifying visible landmark {required_label!r} ({len(data)} bytes)")


def main() -> int:
    output = Path("artifacts/brand-review/android")
    output.mkdir(parents=True, exist_ok=True)

    # Preserve the generated icon exports beside the real emulator screenshots.
    resource_root = Path("src/Client.Android/app/src/main/res")
    export_root = output / "launcher-resources"
    export_root.mkdir(parents=True, exist_ok=True)
    for density in ("mdpi", "hdpi", "xhdpi", "xxhdpi", "xxxhdpi"):
        for name in ("ic_launcher.png", "ic_launcher_round.png", "ic_launcher_foreground.png"):
            source = resource_root / f"mipmap-{density}" / name
            if not source.is_file():
                raise FileNotFoundError(f"Generated Android icon export is missing: {source}")
            destination = export_root / density / name
            destination.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source, destination)

    adaptive_dir = resource_root / "mipmap-anydpi-v26"
    for name in ("ic_launcher.xml", "ic_launcher_round.xml"):
        shutil.copy2(adaptive_dir / name, export_root / name)
    shutil.copy2(resource_root / "values/brand_colors.xml", export_root / "brand_colors.xml")
    shutil.copy2(Path("assets/brand/whitep-coin.svg"), export_root / "whitep-coin.svg")
    shutil.copy2(Path("assets/brand/whitep-design-tokens.json"), export_root / "whitep-design-tokens.json")

    time.sleep(1.0)
    wait_for_text("Start with a request")
    capture(output, "android-home.png", "Start with a request")

    tap_label("Conversations")
    wait_for_text("Conversation history saved on this device.")
    capture(
        output,
        "android-conversations.png",
        "Conversation history saved on this device.",
    )
    tap_label("New conversation")
    wait_for_text("Back to conversations")
    wait_for_text_prefix("Created ")
    capture(
        output,
        "android-conversation-detail.png",
        "Back to conversations",
        required_prefix="Created ",
    )
    scroll_to_text("No messages yet. Write a message below to save it in this conversation.")
    capture(
        output,
        "android-conversation-messages.png",
        "Messages",
        required_prefix="No messages yet.",
    )
    scroll_to_text("Back to conversations", direction="down")
    tap_label("Back to conversations")
    wait_for_text("Conversation history saved on this device.")

    tap_label("Connections")
    wait_for_text("Peer discovery is configured from the New request screen.")
    capture(
        output,
        "android-connections.png",
        "Peer discovery is configured from the New request screen.",
    )
    tap_label("Privacy")
    wait_for_text("Understand the processing-host trust boundary.")
    capture(output, "android-privacy.png", "Understand the processing-host trust boundary.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
