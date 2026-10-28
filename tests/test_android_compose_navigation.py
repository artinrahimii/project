"""Source-contract checks for the runnable Android Compose navigation shell."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ACTIVITY = (
    ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt"
).read_text(encoding="utf-8")
DESTINATIONS = (
    ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/ui/navigation/ClientDestination.kt"
).read_text(encoding="utf-8")
NAVIGATION = (
    ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/ui/navigation/ClientNavigation.kt"
).read_text(encoding="utf-8")
ANDROID_BUILD = (
    ROOT / "src/Client.Android/app/build.gradle.kts"
).read_text(encoding="utf-8")
MANIFEST = (
    ROOT / "src/Client.Android/app/src/main/AndroidManifest.xml"
).read_text(encoding="utf-8")


class AndroidComposeNavigationTests(unittest.TestCase):
    def test_launcher_activity_starts_the_compose_app_and_home_is_the_start_route(self):
        self.assertIn('android.intent.action.MAIN', MANIFEST)
        self.assertIn('android.intent.category.LAUNCHER', MANIFEST)
        self.assertIn("whitePClientApp()", ACTIVITY)
        self.assertIn("startDestination = ClientNavigation.startDestination.route", ACTIVITY)
        self.assertIn("val startDestination: ClientDestination = ClientDestination.HOME", NAVIGATION)

    def test_navigation_compose_dependency_is_explicitly_pinned(self):
        self.assertIn(
            'implementation("androidx.navigation:navigation-compose:2.9.8")',
            ANDROID_BUILD,
        )
        self.assertIn("rememberNavController()", ACTIVITY)
        self.assertIn("NavHost(", ACTIVITY)
        self.assertIn("currentBackStackEntryAsState()", ACTIVITY)

    def test_every_route_is_registered_and_top_level_navigation_is_reachable(self):
        for destination in ("HOME", "REQUEST", "CONNECTIONS", "PRIVACY"):
            with self.subTest(destination=destination):
                self.assertIn(f"{destination}(", DESTINATIONS)
                self.assertIn(f"composable(ClientDestination.{destination}.route)", ACTIVITY)
        self.assertIn("ClientNavigation.bottomNavigation.forEach", ACTIVITY)
        self.assertIn("val bottomNavigation: List<ClientDestination>", NAVIGATION)
        self.assertIn("ClientDestination.HOME", NAVIGATION)
        self.assertIn("ClientDestination.CONNECTIONS", NAVIGATION)
        self.assertIn("ClientDestination.PRIVACY", NAVIGATION)
        self.assertNotIn("ClientDestination.REQUEST", NAVIGATION)
        self.assertIn("navController.navigate(item.route)", ACTIVITY)
        self.assertIn("popUpTo(navController.graph.findStartDestination().id)", ACTIVITY)
        self.assertIn("launchSingleTop = true", ACTIVITY)
        self.assertIn("restoreState = true", ACTIVITY)

    def test_request_screen_does_not_claim_to_send_and_privacy_screen_discloses_host_visibility(self):
        self.assertIn("Android-to-host transport is not implemented.", ACTIVITY)
        self.assertIn("The processing host can read your prompt and context capsule", ACTIVITY)
        self.assertIn("Transport encryption protects data in transit, not from the receiving host.", ACTIVITY)
        self.assertIn("Android-to-Windows delivery", ACTIVITY)
        self.assertIn("no-storage behavior have not been verified.", ACTIVITY)


if __name__ == "__main__":
    unittest.main()
