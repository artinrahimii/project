import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
README = (ROOT / "README.md").read_text(encoding="utf-8")

class ReadmeContractTests(unittest.TestCase):
    def test_product_goal_is_documented(self):
        self.assertIn("هدف محصول", README)
        self.assertIn("P2P", README)

    def test_first_release_components_are_listed(self):
        for term in ("میزبان ویندوز", "کلاینت اندروید", "قراردادهای مشترک", "موتور محلی", "آزمون و CI"):
            with self.subTest(term=term):
                self.assertIn(term, README)

    def test_current_missing_build_projects_are_disclosed(self):
        self.assertIn("هنوز در مخزن ایجاد نشده‌اند", README)
        self.assertIn("build محصول", README)

    def test_privacy_goals_are_not_claimed_as_proven(self):
        self.assertIn("الزامات طراحی‌اند، نه ادعاهای اثبات‌شده", README)

    def test_current_python_checks_are_distinguished_from_product_build(self):
        self.assertIn("python -m unittest discover -s tests -v", README)
        self.assertIn("جایگزین build میزبان ویندوز", README)

if __name__ == "__main__":
    unittest.main()
