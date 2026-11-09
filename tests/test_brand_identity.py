from __future__ import annotations
import json, struct, tempfile, unittest
from pathlib import Path
from xml.etree import ElementTree as ET
from tools.capture_android_brand_review import parse_bounds, ui_tree_has_text, ui_tree_has_text_prefix
from tools.generate_brand_assets import EXPECTED, generate_brand_assets
ROOT=Path(__file__).resolve().parents[1]
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
        brand = (ROOT / "src/Host.Windows/Assets/BrandColors.xaml").read_text(encoding="utf-8")
        self.assertIn('<Setter Property="ContentTemplate">', brand)
        self.assertIn('<DataTemplate><TextBlock Text="{Binding}" Foreground="{StaticResource PureWhiteBrush}"/></DataTemplate>', brand)

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
