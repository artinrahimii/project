"""Contract checks for real Windows OS, physical-memory and system-drive discovery."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SERVICE = (ROOT / "src/Host.Windows/WindowsHostHardwareDiscovery.cs").read_text(encoding="utf-8")
APP_CODE = (ROOT / "src/Host.Windows/App.xaml.cs").read_text(encoding="utf-8")


class WindowsHardwareDiscoveryTests(unittest.TestCase):
    def test_service_is_windows_only_and_reads_os_description_and_version(self):
        self.assertIn("OperatingSystem.IsWindows()", SERVICE)
        self.assertIn("RuntimeInformation.OSDescription", SERVICE)
        self.assertIn("Environment.OSVersion.Version", SERVICE)
        self.assertIn("OperatingSystemDescription", SERVICE)
        self.assertIn("OperatingSystemVersion", SERVICE)

    def test_physical_memory_comes_from_the_real_windows_api(self):
        self.assertIn('LibraryImport("kernel32.dll", SetLastError = true)', SERVICE)
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


if __name__ == "__main__":
    unittest.main()
