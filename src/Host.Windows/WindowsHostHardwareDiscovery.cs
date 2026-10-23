using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace WhiteP.Host.Windows;

public sealed record WindowsHostHardwareSnapshot(
    string OperatingSystemDescription,
    Version OperatingSystemVersion,
    string SystemDriveName,
    ulong TotalPhysicalMemoryBytes,
    long SystemDriveTotalBytes,
    long SystemDriveAvailableBytes);

public static class WindowsHostHardwareDiscovery
{
    public static WindowsHostHardwareSnapshot Discover()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Hardware discovery in this service requires Windows.");
        }

        var operatingSystemDescription = RuntimeInformation.OSDescription;
        var operatingSystemVersion = Environment.OSVersion.Version;
        if (string.IsNullOrWhiteSpace(operatingSystemDescription) || operatingSystemVersion.Major < 1)
        {
            throw new InvalidOperationException("Windows operating system information is unavailable.");
        }

        var memoryStatus = new MemoryStatusEx
        {
            Length = checked((uint)Marshal.SizeOf<MemoryStatusEx>())
        };
        if (!NativeMethods.GlobalMemoryStatusEx(ref memoryStatus))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Windows could not report physical memory.");
        }

        if (memoryStatus.TotalPhysical == 0)
        {
            throw new InvalidOperationException("Windows returned an invalid total physical memory value.");
        }

        var windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var systemDriveRoot = Path.GetPathRoot(windowsDirectory);
        if (string.IsNullOrWhiteSpace(systemDriveRoot))
        {
            throw new InvalidOperationException("Windows did not report a valid system drive.");
        }

        var systemDrive = new DriveInfo(systemDriveRoot);
        if (!systemDrive.IsReady)
        {
            throw new IOException("The Windows system drive is not ready.");
        }

        var totalSystemDriveBytes = systemDrive.TotalSize;
        var availableSystemDriveBytes = systemDrive.AvailableFreeSpace;
        if (totalSystemDriveBytes <= 0 ||
            availableSystemDriveBytes < 0 ||
            availableSystemDriveBytes > totalSystemDriveBytes)
        {
            throw new InvalidOperationException("Windows returned invalid system-drive capacity values.");
        }

        return new WindowsHostHardwareSnapshot(
            operatingSystemDescription,
            operatingSystemVersion,
            systemDrive.Name,
            memoryStatus.TotalPhysical,
            totalSystemDriveBytes,
            availableSystemDriveBytes);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhysical;
        public ulong AvailablePhysical;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;
    }

    private static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);
    }
}
