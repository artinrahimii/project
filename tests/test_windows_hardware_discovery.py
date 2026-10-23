"""Contract checks for real Windows OS, physical-memory and system-drive discovery."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SERVICE = (ROOT / "src/Host.Windows/WindowsHostHardwareDiscovery.cs").read_text(encoding="utf-8")
APP_CODE = (ROOT / "src/Host.Windows/App.xaml.cs").read_text(encoding="utf-8")
WINDOW_CODE = (ROOT / "src/Host.Windows/MainWindow.xaml.cs").read_text(encoding="utf-8")
XAML = (ROOT / "src/Host.Windows/MainWindow.xaml").read_text(encoding="utf-8")
ESTIMATOR = (ROOT / "src/Host.Windows/ModelMemoryCompatibilityEstimator.cs").read_text(encoding="utf-8")


class WindowsHardwareDiscoveryTests(unittest.TestCase):
    def test_service_is_windows_only_and_reads_os_description_and_version(self):
        self.assertIn("OperatingSystem.IsWindows()", SERVICE)
        self.assertIn("RuntimeInformation.OSDescription", SERVICE)
        self.assertIn("Environment.OSVersion.Version", SERVICE)
        self.assertIn("OperatingSystemDescription", SERVICE)
        self.assertIn("OperatingSystemVersion", SERVICE)

    def test_display_adapters_are_enumerated_through_the_real_windows_api(self):
        self.assertIn("EnumDisplayDevicesW", SERVICE)
        self.assertIn("DiscoverDisplayAdapters()", SERVICE)
        self.assertIn("DisplayDeviceAttachedToDesktop", SERVICE)
        self.assertIn("DisplayDeviceMirroringDriver", SERVICE)
        self.assertIn("WindowsHostHardwareDiscovery.DiscoverDisplayAdapters()", APP_CODE)
        self.assertIn("GPU/display adapters", (ROOT / "src/Host.Windows/MainWindow.xaml.cs").read_text(encoding="utf-8"))

    def test_physical_memory_comes_from_the_real_windows_api(self):
        self.assertIn('DllImport("kernel32.dll", SetLastError = true)', SERVICE)
        self.assertIn("GlobalMemoryStatusEx", SERVICE)
        self.assertIn("Marshal.SizeOf<MemoryStatusEx>()", SERVICE)
        self.assertIn("memoryStatus.TotalPhysical", SERVICE)
        self.assertIn("TotalPhysicalMemoryBytes", SERVICE)
        self.assertIn("Win32Exception(Marshal.GetLastWin32Error()", SERVICE)

    def test_system_drive_space_is_read_and_sanity_checked(self):
        self.assertIn("Environment.SpecialFolder.Windows", SERVICE)
        self.assertIn("Path.GetPathRoot(windowsDirectory)", SERVICE)
        self.assertIn("new DriveInfo(systemDriveRoot)", SERVICE)
        self.assertIn("systemDrive.IsReady", SERVICE)
        self.assertIn("systemDrive.TotalSize", SERVICE)
        self.assertIn("systemDrive.AvailableFreeSpace", SERVICE)
        self.assertIn("availableSystemDriveBytes > totalSystemDriveBytes", SERVICE)

    def test_actual_windows_startup_smoke_invokes_and_validates_the_service(self):
        self.assertIn("WindowsHostHardwareDiscovery.Discover()", APP_CODE)
        for marker in (
            "hardware.OperatingSystemDescription",
            "hardware.OperatingSystemVersion.Major",
            "hardware.TotalPhysicalMemoryBytes",
            "hardware.SystemDriveName",
            "hardware.SystemDriveTotalBytes",
            "hardware.SystemDriveAvailableBytes",
            "Shutdown(3);",
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker, APP_CODE)



class ModelMemoryCompatibilityTests(unittest.TestCase):
    def test_estimator_uses_live_available_physical_memory(self):
        self.assertIn("AvailablePhysicalMemoryBytes", SERVICE)
        self.assertIn("memoryStatus.AvailablePhysical", SERVICE)
        self.assertIn("ModelMemoryCompatibilityEstimator.Estimate", WINDOW_CODE)
        self.assertIn("(ulong)requiredMemoryBytes <= availablePhysicalMemoryBytes", ESTIMATOR)

    def test_estimator_rejects_invalid_declared_requirement(self):
        self.assertIn("requiredMemoryBytes <= 0", ESTIMATOR)
        self.assertIn("ArgumentOutOfRangeException", ESTIMATOR)
        self.assertIn("double.IsFinite(requiredGiB)", WINDOW_CODE)

    def test_wpf_exposes_estimate_and_discloses_limits(self):
        self.assertIn('x:Name="ModelMemoryRequirementGiB"', XAML)
        self.assertIn('Click="EstimateModelCompatibility_Click"', XAML)
        self.assertIn('x:Name="ModelCompatibilityStatus"', XAML)
        self.assertIn("GPU/VRAM", ESTIMATOR)


if __name__ == "__main__":
    unittest.main()
