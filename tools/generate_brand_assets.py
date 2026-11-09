#!/usr/bin/env python3
"""Generate platform icons and color resources from one canonical SVG and token JSON."""
from __future__ import annotations

import argparse
import json
import math
import re
import struct
import xml.etree.ElementTree as ET
import zlib
from pathlib import Path

EXPECTED = {
    "midnight_ink": "#142537",
    "porcelain": "#F4F0E8",
    "pure_white": "#FFFFFF",
    "antique_gold": "#B89A62",
    "muted_teal": "#587C80",
}
DENSITIES = {
    "mdpi": (48, 108), "hdpi": (72, 162), "xhdpi": (96, 216),
    "xxhdpi": (144, 324), "xxxhdpi": (192, 432),
}
SAMPLES = 4


def _rgb(value: str) -> tuple[int, int, int]:
    if not re.fullmatch(r"#[0-9A-Fa-f]{6}", value):
        raise ValueError(f"Unsupported SVG color: {value}")
    return tuple(int(value[i:i + 2], 16) for i in (1, 3, 5))


def _path_polygons(data: str) -> list[list[tuple[float, float]]]:
    tokens = re.findall(r"[MLHVZ]|-?(?:\d+(?:\.\d*)?|\.\d+)", data)
    polygons: list[list[tuple[float, float]]] = []
    polygon: list[tuple[float, float]] = []
    command = None
    i = 0
    x = y = 0.0
    while i < len(tokens):
        if tokens[i] in {"M", "L", "H", "V", "Z"}:
            command = tokens[i]
            i += 1
            if command == "Z":
                if polygon:
                    polygons.append(polygon)
                    polygon = []
                command = None
                continue
        if command in {"M", "L"}:
            x, y = float(tokens[i]), float(tokens[i + 1])
            i += 2
            polygon.append((x, y))
            if command == "M":
                command = "L"
        elif command == "H":
            x = float(tokens[i])
            i += 1
            polygon.append((x, y))
        elif command == "V":
            y = float(tokens[i])
            i += 1
            polygon.append((x, y))
        else:
            raise ValueError(f"Malformed SVG path near {tokens[i:i + 3]}")
    if polygon:
        polygons.append(polygon)
    if not polygons or any(len(item) < 3 for item in polygons):
        raise ValueError("SVG path must contain polygonal subpaths")
    return polygons


def _collect_shapes(svg_path: Path, group_id: str | None = None):
    root = ET.parse(svg_path).getroot()
    view_box = [float(value) for value in root.attrib["viewBox"].split()]
    groups = {node.attrib.get("id"): node for node in root.iter() if node.tag.endswith("g")}
    if group_id is not None and group_id not in groups:
        raise ValueError(f"Missing SVG group: {group_id}")
    selected = [groups[group_id]] if group_id else [node for node in root if node.tag.endswith("g")]
    shapes = []
    for group in selected:
        for node in group.iter():
            tag = node.tag.rsplit("}", 1)[-1]
            if tag == "rect":
                shapes.append({
                    "type": "rect", "x": float(node.attrib.get("x", 0)),
                    "y": float(node.attrib.get("y", 0)), "width": float(node.attrib["width"]),
                    "height": float(node.attrib["height"]), "fill": _rgb(node.attrib["fill"]),
                })
            elif tag == "circle":
                shapes.append({
                    "type": "circle", "cx": float(node.attrib["cx"]), "cy": float(node.attrib["cy"]),
                    "radius": float(node.attrib["r"]), "fill": _rgb(node.attrib["fill"]),
                    "stroke": _rgb(node.attrib["stroke"]) if node.attrib.get("stroke") else None,
                    "stroke_width": float(node.attrib.get("stroke-width", 0)),
                })
            elif tag == "path":
                shapes.append({
                    "type": "path", "polygons": _path_polygons(node.attrib["d"]),
                    "fill": _rgb(node.attrib["fill"]),
                    "fill_rule": node.attrib.get("fill-rule", "nonzero"),
                })
    if not shapes:
        raise ValueError(f"No renderable SVG shapes in {group_id or 'root'}")
    return view_box[2], view_box[3], shapes


def _inside_polygon(point, polygon) -> bool:
    x, y = point
    inside = False
    j = len(polygon) - 1
    for i, (xi, yi) in enumerate(polygon):
        xj, yj = polygon[j]
        if ((yi > y) != (yj > y)) and x < (xj - xi) * (y - yi) / ((yj - yi) or 1e-12) + xi:
            inside = not inside
        j = i
    return inside


def _sample_color(shape, point):
    x, y = point
    if shape["type"] == "rect":
        return shape["fill"] if (
            shape["x"] <= x <= shape["x"] + shape["width"]
            and shape["y"] <= y <= shape["y"] + shape["height"]
        ) else None
    if shape["type"] == "circle":
        distance = math.hypot(x - shape["cx"], y - shape["cy"])
        if distance <= shape["radius"]:
            if shape["stroke"] and abs(distance - shape["radius"]) <= shape["stroke_width"] / 2:
                return shape["stroke"]
            return shape["fill"]
        return None
    hits = sum(_inside_polygon(point, polygon) for polygon in shape["polygons"])
    filled = hits % 2 if shape["fill_rule"] == "evenodd" else hits > 0
    return shape["fill"] if filled else None


def _render_png(svg_path: Path, size: int, group_id: str | None = None) -> bytes:
    width, height, shapes = _collect_shapes(svg_path, group_id)
    pixels = bytearray()
    sample_count = SAMPLES * SAMPLES
    for py in range(size):
        pixels.append(0)  # PNG filter: None
        for px in range(size):
            red = green = blue = alpha = 0
            for sy in range(SAMPLES):
                for sx in range(SAMPLES):
                    point = (
                        (px + (sx + 0.5) / SAMPLES) * width / size,
                        (py + (sy + 0.5) / SAMPLES) * height / size,
                    )
                    color = None
                    for shape in reversed(shapes):
                        color = _sample_color(shape, point)
                        if color is not None:
                            break
                    if color is not None:
                        red += color[0]
                        green += color[1]
                        blue += color[2]
                        alpha += 255
            if alpha:
                pixels.extend((
                    round(red * 255 / alpha), round(green * 255 / alpha),
                    round(blue * 255 / alpha), round(alpha / sample_count),
                ))
            else:
                pixels.extend((0, 0, 0, 0))

    def png_chunk(kind: bytes, data: bytes) -> bytes:
        crc = zlib.crc32(kind + data) & 0xFFFFFFFF
        return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", crc)

    header = struct.pack(">IIBBBBB", size, size, 8, 6, 0, 0, 0)
    return (
        b"\x89PNG\r\n\x1a\n"
        + png_chunk(b"IHDR", header)
        + png_chunk(b"IDAT", zlib.compress(bytes(pixels), 9))
        + png_chunk(b"IEND", b"")
    )


def _write(path: Path, content: bytes | str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    if isinstance(content, str):
        path.write_text(content, encoding="utf-8", newline="\n")
    else:
        path.write_bytes(content)


def _make_ico(images: list[tuple[int, bytes]]) -> bytes:
    header = struct.pack("<HHH", 0, 1, len(images))
    directory = bytearray()
    payload = bytearray()
    offset = 6 + 16 * len(images)
    for size, data in images:
        dimension = 0 if size == 256 else size
        directory.extend(struct.pack("<BBBBHHII", dimension, dimension, 0, 0, 1, 32, len(data), offset))
        payload.extend(data)
        offset += len(data)
    return header + bytes(directory) + bytes(payload)


def _make_kotlin_colors(colors: dict[str, str]) -> str:
    names = {
        "midnight_ink": "midnightInk", "porcelain": "porcelain",
        "pure_white": "pureWhite", "antique_gold": "antiqueGold", "muted_teal": "mutedTeal",
    }
    return (
        "package com.whitep.client.ui.theme\n\n"
        "import androidx.compose.ui.graphics.Color\n\n"
        "/** Generated from assets/brand/whitep-design-tokens.json. Do not edit by hand. */\n"
        "internal object WhitePBrandColors {\n"
        + "".join(f"    val {names[key]} = Color(0xFF{value[1:]})\n" for key, value in colors.items())
        + "}\n"
    )


def _make_android_colors_xml(colors: dict[str, str]) -> str:
    return (
        '<?xml version="1.0" encoding="utf-8"?>\n<resources>\n'
        + "".join(f'    <color name="whitep_{key}">{value}</color>\n' for key, value in colors.items())
        + "</resources>\n"
    )


def _make_wpf_dictionary(colors: dict[str, str]) -> str:
    display = {
        "midnight_ink": "MidnightInk", "porcelain": "Porcelain",
        "pure_white": "PureWhite", "antique_gold": "AntiqueGold", "muted_teal": "MutedTeal",
    }
    output = [
        '<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" '
        'xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">\n'
    ]
    for key, value in colors.items():
        name = display[key]
        output.append(f'  <Color x:Key="{name}Color">{value}</Color>\n')
        output.append(f'  <SolidColorBrush x:Key="{name}Brush" Color="{{StaticResource {name}Color}}"/>\n')
    output.extend([
        '  <Style TargetType="{x:Type TextBlock}"><Setter Property="Foreground" Value="{StaticResource MidnightInkBrush}"/></Style>\n',
        '  <Style TargetType="{x:Type Button}"><Setter Property="Background" Value="{StaticResource MidnightInkBrush}"/><Setter Property="Foreground" Value="{StaticResource PureWhiteBrush}"/><Setter Property="BorderBrush" Value="{StaticResource MidnightInkBrush}"/><Setter Property="BorderThickness" Value="1"/><Setter Property="Padding" Value="12,6"/><Setter Property="MinHeight" Value="30"/></Style>\n',
        '  <Style TargetType="{x:Type TextBox}"><Setter Property="Background" Value="{StaticResource PureWhiteBrush}"/><Setter Property="Foreground" Value="{StaticResource MidnightInkBrush}"/><Setter Property="CaretBrush" Value="{StaticResource MidnightInkBrush}"/><Setter Property="BorderBrush" Value="{StaticResource MutedTealBrush}"/><Setter Property="BorderThickness" Value="1"/><Setter Property="Padding" Value="6,4"/></Style>\n',
        '  <Style TargetType="{x:Type CheckBox}"><Setter Property="Foreground" Value="{StaticResource MidnightInkBrush}"/><Setter Property="Background" Value="{StaticResource PureWhiteBrush}"/></Style>\n',
        '  <Style TargetType="{x:Type GroupBox}"><Setter Property="Foreground" Value="{StaticResource MidnightInkBrush}"/><Setter Property="BorderBrush" Value="{StaticResource MutedTealBrush}"/></Style>\n',
        '  <Style TargetType="{x:Type ListBox}"><Setter Property="Background" Value="{StaticResource PureWhiteBrush}"/><Setter Property="Foreground" Value="{StaticResource MidnightInkBrush}"/><Setter Property="BorderBrush" Value="{StaticResource MutedTealBrush}"/></Style>\n',
        '  <Style TargetType="{x:Type ListView}"><Setter Property="Background" Value="{StaticResource PureWhiteBrush}"/><Setter Property="Foreground" Value="{StaticResource MidnightInkBrush}"/><Setter Property="BorderBrush" Value="{StaticResource MutedTealBrush}"/></Style>\n',
        '</ResourceDictionary>\n',
    ])
    return "".join(output)


def generate_brand_assets(repo_root: Path, output_root: Path) -> list[str]:
    repo_root = Path(repo_root).resolve()
    output_root = Path(output_root).resolve()
    tokens = json.loads((repo_root / "assets/brand/whitep-design-tokens.json").read_text(encoding="utf-8"))
    items = tokens.get("colors", [])
    colors = {item["token"]: item["hex"] for item in items}
    if len(items) != 5 or colors != EXPECTED:
        raise ValueError(f"Palette must equal exactly the approved five colors: {EXPECTED}")

    svg = repo_root / "assets/brand/whitep-coin.svg"
    ico_path = output_root / "src/Host.Windows/Assets/WhiteP.ico"
    _write(ico_path, _make_ico([(size, _render_png(svg, size)) for size in (16, 24, 32, 48, 64, 128, 256)]))
    generated = [str(ico_path.relative_to(output_root))]

    android_res = output_root / "src/Client.Android/app/src/main/res"
    for density, (legacy_size, foreground_size) in DENSITIES.items():
        folder = android_res / f"mipmap-{density}"
        full_icon = _render_png(svg, legacy_size)
        foreground = _render_png(svg, foreground_size, "foreground")
        for name, data in (
            ("ic_launcher.png", full_icon),
            ("ic_launcher_round.png", full_icon),
            ("ic_launcher_foreground.png", foreground),
        ):
            path = folder / name
            _write(path, data)
            generated.append(str(path.relative_to(output_root)))

    adaptive = (
        '<?xml version="1.0" encoding="utf-8"?>\n'
        '<adaptive-icon xmlns:android="http://schemas.android.com/apk/res/android">\n'
        '  <background android:drawable="@color/whitep_midnight_ink"/>\n'
        '  <foreground android:drawable="@mipmap/ic_launcher_foreground"/>\n'
        '</adaptive-icon>\n'
    )
    for name in ("ic_launcher.xml", "ic_launcher_round.xml"):
        path = android_res / "mipmap-anydpi-v26" / name
        _write(path, adaptive)
        generated.append(str(path.relative_to(output_root)))

    outputs = (
        (android_res / "values/brand_colors.xml", _make_android_colors_xml(colors)),
        (output_root / "src/Client.Android/app/src/main/java/com/whitep/client/ui/theme/WhitePBrandColors.kt", _make_kotlin_colors(colors)),
        (output_root / "src/Host.Windows/Assets/BrandColors.xaml", _make_wpf_dictionary(colors)),
    )
    for path, content in outputs:
        _write(path, content)
        generated.append(str(path.relative_to(output_root)))
    return generated


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--output-root", type=Path, default=None)
    args = parser.parse_args()
    output_root = args.output_root or args.repo_root
    outputs = generate_brand_assets(args.repo_root, output_root)
    print(f"Generated {len(outputs)} brand assets from canonical sources.")
    for path in outputs:
        print(path)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
