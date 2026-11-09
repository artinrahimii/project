from __future__ import annotations
import json, struct, tempfile, unittest
from pathlib import Path
from xml.etree import ElementTree as ET
from tools.capture_android_brand_review import parse_bounds, ui_tree_has_text, ui_tree_has_text_prefix
from tools.generate_brand_assets import EXPECTED, generate_brand_assets
ROOT=Path(__file__).resolve().parents[1]

def _relative_luminance(hex_color: str) -> float:
    channels = [int(hex_color[index:index + 2], 16) / 255 for index in (1, 3, 5)]
    linear = [
        channel / 12.92 if channel <= 0.04045 else ((channel + 0.055) / 1.055) ** 2.4
        for channel in channels
    ]
    return 0.2126 * linear[0] + 0.7152 * linear[1] + 0.0722 * linear[2]


def _contrast_ratio(foreground: str, background: str) -> float:
    lighter, darker = sorted(
        (_relative_luminance(foreground), _relative_luminance(background)),
        reverse=True,
    )
    return (lighter + 0.05) / (darker + 0.05)
class BrandIdentityTests(unittest.TestCase):
    def test_android_review_capture_parses_uiautomator_bounds(self):
        self.assertEqual(parse_bounds("[0,10][100,50]"), (50, 30))
        with self.assertRaises(ValueError):
            parse_bounds("invalid")

    def test_review_capture_checks_exact_visible_screen_landmarks(self):
        root = ET.fromstring(
            '<hierarchy><node text="Conversations" />'
            '<node content-desc="Privacy" />'
            '<node text="Created Nov 9, 2026" /></hierarchy>'
        )
        self.assertTrue(ui_tree_has_text(root, "Conversations"))
        self.assertTrue(ui_tree_has_text(root, "Privacy"))
        self.assertFalse(ui_tree_has_text(root, "Conversation detail"))
        self.assertTrue(ui_tree_has_text_prefix(root, "Created "))

        capture = (ROOT / "tools/capture_android_brand_review.py").read_text(encoding="utf-8")
        self.assertIn('wait_for_text("Conversation history saved on this device.")', capture)
        self.assertIn('wait_for_text_prefix("Created ")', capture)
        self.assertIn('scroll_to_text("No messages yet. Write a message below to save it in this conversation.")', capture)
        self.assertIn('capture(\n        output,\n        "android-conversation-messages.png",', capture)
        self.assertIn('wait_for_text("Peer discovery is configured from the New request screen.")', capture)
        self.assertIn('wait_for_text("Understand the processing-host trust boundary.")', capture)
        self.assertIn("Refusing to capture", capture)

    def test_shared_palette_contrast_and_semantic_roles_meet_thresholds(self):
        tokens = json.loads(
            (ROOT / "assets/brand/whitep-design-tokens.json").read_text(encoding="utf-8")
        )
        palette = {item["token"]: item["hex"] for item in tokens["colors"]}
        ink = palette["midnight_ink"]
        paper = palette["porcelain"]
        white = palette["pure_white"]
        gold = palette["antique_gold"]
        teal = palette["muted_teal"]

        self.assertGreaterEqual(_contrast_ratio(ink, paper), 4.5)
        self.assertGreaterEqual(_contrast_ratio(ink, white), 4.5)
        self.assertGreaterEqual(_contrast_ratio(white, ink), 4.5)
        self.assertGreaterEqual(_contrast_ratio(white, teal), 4.5)
        self.assertGreaterEqual(_contrast_ratio(teal, paper), 3.0)
        self.assertLess(_contrast_ratio(teal, paper), 4.5)
        self.assertLess(_contrast_ratio(gold, paper), 3.0)
        self.assertLess(_contrast_ratio(gold, white), 3.0)
        self.assertGreaterEqual(_contrast_ratio(ink, gold), 4.5)

        roles = tokens["semantic_roles"]
        self.assertEqual(roles["primary_text"], "midnight_ink")
        self.assertEqual(roles["secondary_text"], "midnight_ink")
        self.assertEqual(roles["decorative_accent"], "antique_gold")

        android_theme = (
            ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/ui/theme/Theme.kt"
        ).read_text(encoding="utf-8")
        self.assertIn("primary = ink", android_theme)
        self.assertIn("onPrimary = white", android_theme)
        self.assertIn("secondary = teal", android_theme)
        self.assertIn("onSecondary = white", android_theme)
        self.assertIn("onSecondaryContainer = ink", android_theme)
        self.assertIn("onBackground = ink", android_theme)
        self.assertIn("onSurface = ink", android_theme)

        wpf_theme = (ROOT / "src/Host.Windows/Assets/BrandColors.xaml").read_text(encoding="utf-8")
        self.assertIn('TargetType="{x:Type TextBlock}"><Setter Property="Foreground" Value="{StaticResource MidnightInkBrush}"', wpf_theme)
        self.assertIn('TargetType="{x:Type Button}"><Setter Property="Background" Value="{StaticResource MidnightInkBrush}"', wpf_theme)
        self.assertIn('<DataTemplate><TextBlock Text="{Binding}" Foreground="{StaticResource PureWhiteBrush}"/></DataTemplate>', wpf_theme)
        self.assertIn('<Setter Property="BorderBrush" Value="{StaticResource MutedTealBrush}"/>', wpf_theme)

    def test_canonical_tokens_are_exactly_the_approved_five_colors(self):
        doc=json.loads((ROOT/"assets/brand/whitep-design-tokens.json").read_text(encoding="utf-8"))
        self.assertEqual({x["token"]:x["hex"] for x in doc["colors"]},EXPECTED)
        self.assertEqual(len(doc["colors"]),5)
        self.assertEqual(doc["semantic_roles"]["secondary_text"],"midnight_ink")
    def test_one_master_vector_generates_windows_and_android_icons(self):
        with tempfile.TemporaryDirectory() as temp:
            root=Path(temp); generated=generate_brand_assets(ROOT,root)
            ico=(root/"src/Host.Windows/Assets/WhiteP.ico").read_bytes()
            self.assertEqual(struct.unpack_from("<HHH",ico,0),(0,1,7))
            for density,(legacy,foreground) in {"mdpi":(48,108),"hdpi":(72,162),"xhdpi":(96,216),"xxhdpi":(144,324),"xxxhdpi":(192,432)}.items():
                folder=root/f"src/Client.Android/app/src/main/res/mipmap-{density}"
                for name,size in (("ic_launcher.png",legacy),("ic_launcher_round.png",legacy),("ic_launcher_foreground.png",foreground)):
                    data=(folder/name).read_bytes()
                    self.assertEqual(data[:8],b"\x89PNG\r\n\x1a\n")
                    self.assertEqual(struct.unpack_from(">II",data,16),(size,size))
            adaptive=root/"src/Client.Android/app/src/main/res/mipmap-anydpi-v26/ic_launcher.xml"
            self.assertIn("@mipmap/ic_launcher_foreground",adaptive.read_text(encoding="utf-8"))
            self.assertIn("src/Host.Windows/Assets/WhiteP.ico",generated)
    def test_platform_color_resources_share_the_token_file(self):
        with tempfile.TemporaryDirectory() as temp:
            root=Path(temp); generate_brand_assets(ROOT,root)
            paths=[root/"src/Client.Android/app/src/main/java/com/whitep/client/ui/theme/WhitePBrandColors.kt",
                   root/"src/Client.Android/app/src/main/res/values/brand_colors.xml",
                   root/"src/Host.Windows/Assets/BrandColors.xaml"]
            for path in paths:
                content=path.read_text(encoding="utf-8")
                for value in EXPECTED.values():
                    if path.suffix == ".kt":
                        self.assertIn(f"Color(0xFF{value[1:]})",content)
                    else:
                        self.assertIn(value,content)
    def test_both_platforms_load_the_shared_theme_and_icon(self):
        main=(ROOT/"src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt").read_text(encoding="utf-8")
        theme=(ROOT/"src/Client.Android/app/src/main/java/com/whitep/client/ui/theme/Theme.kt").read_text(encoding="utf-8")
        manifest=(ROOT/"src/Client.Android/app/src/main/AndroidManifest.xml").read_text(encoding="utf-8")
        app=(ROOT/"src/Host.Windows/App.xaml").read_text(encoding="utf-8")
        window=(ROOT/"src/Host.Windows/MainWindow.xaml").read_text(encoding="utf-8")
        project=(ROOT/"src/Host.Windows/WhiteP.Host.Windows.csproj").read_text(encoding="utf-8")
        self.assertIn("whitePTheme {",main); self.assertIn("lightColorScheme",theme)
        self.assertIn('android:icon="@mipmap/ic_launcher"',manifest)
        self.assertIn('android:roundIcon="@mipmap/ic_launcher_round"',manifest)
        self.assertIn('Source="Assets/BrandColors.xaml"',app)
        self.assertIn('Icon="Assets/WhiteP.ico"',window)
        self.assertIn(r"<ApplicationIcon>Assets\WhiteP.ico</ApplicationIcon>",project)
    def test_wpf_button_string_content_renders_with_contrasting_foreground(self):
        with tempfile.TemporaryDirectory() as temp:
            output = Path(temp)
            generate_brand_assets(ROOT, output)
            generated = (
                output / "src/Host.Windows/Assets/BrandColors.xaml"
            ).read_text(encoding="utf-8")
            tracked = (
                ROOT / "src/Host.Windows/Assets/BrandColors.xaml"
            ).read_text(encoding="utf-8")
            for candidate in (generated, tracked):
                parsed = ET.fromstring(candidate)
                self.assertTrue(
                    any(
                        element.attrib.get("Property") == "ContentTemplate"
                        for element in parsed.iter()
                    )
                )
                self.assertIn('<Setter Property="ContentTemplate">', candidate)
                self.assertIn(
                    '<DataTemplate><TextBlock Text="{Binding}" Foreground="{StaticResource PureWhiteBrush}"/></DataTemplate>',
                    candidate,
                )

    def test_visual_review_capture_keeps_generated_icon_exports(self):
        capture = (ROOT / "tools/capture_android_brand_review.py").read_text(encoding="utf-8")
        workflow = (ROOT / ".github/workflows/product-build.yml").read_text(encoding="utf-8")
        self.assertIn('output / "launcher-resources"', capture)
        self.assertIn('"ic_launcher_foreground.png"', capture)
        self.assertIn('"whitep-coin.svg"', capture)
        self.assertIn('"whitep-design-tokens.json"', capture)
        self.assertIn('Copy-Item -LiteralPath "src/Host.Windows/Assets/WhiteP.ico"', workflow)
        self.assertIn('path: artifacts/brand-review/windows/', workflow)

    def test_clean_build_workflows_generate_brand_assets(self):
        build=(ROOT/".github/workflows/product-build.yml").read_text(encoding="utf-8")
        release=(ROOT/".github/workflows/android-release.yml").read_text(encoding="utf-8")
        self.assertEqual(build.count("python tools/generate_brand_assets.py"),2)
        self.assertIn("python tools/generate_brand_assets.py",release)
if __name__=="__main__": unittest.main()
