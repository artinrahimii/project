"""Guard persisted, measurable owner resource limits in the Windows host."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def source(relative_path: str) -> str:
    return (ROOT / relative_path).read_text(encoding="utf-8")

class HostResourceSettingsContractTests(unittest.TestCase):
    def test_resource_limits_are_schema_versioned_validated_and_persisted(self):
        store = source("src/Host.Infrastructure/LocalHostSettingsStore.cs")
        self.assertIn("CurrentSchemaVersion = 2", store)
        self.assertIn("document.SchemaVersion != 1", store)
        self.assertIn("document.SchemaVersion == 1", store)
        self.assertIn("Defaults.RequiredModelMemoryGiB", store)
        self.assertIn("Defaults.MinimumFreeDiskGiB", store)
        self.assertIn('JsonPropertyName("requiredModelMemoryGiB")', store)
        self.assertIn('JsonPropertyName("minimumFreeDiskGiB")', store)
        self.assertIn("RequiredModelMemoryGiB = validated.RequiredModelMemoryGiB", store)
        self.assertIn("MinimumFreeDiskGiB = validated.MinimumFreeDiskGiB", store)
        self.assertIn("double.IsFinite(requiredGiB)", store)

    def test_wpf_restores_limits_and_persists_before_inference_or_hosting(self):
        window = source("src/Host.Windows/MainWindow.xaml.cs")
        xaml = source("src/Host.Windows/MainWindow.xaml")
        self.assertIn("ModelMemoryRequirementGiB.Text = _settings.RequiredModelMemoryGiB", window)
        self.assertIn("MinimumFreeDiskGiB.Text = _settings.MinimumFreeDiskGiB", window)
        self.assertIn("private bool TryPersistCapacityRequirements()", window)
        self.assertIn("RequiredModelMemoryGiB = requiredModelMemoryGiB", window)
        self.assertIn("MinimumFreeDiskGiB = minimumFreeDiskGiB", window)
        self.assertIn("private void CapacityRequirement_LostFocus", window)
        self.assertEqual(2, xaml.count('LostFocus="CapacityRequirement_LostFocus"'))
        self.assertIn("TryPersistCapacityRequirements()", window)

    def test_only_live_measured_ram_and_disk_are_claimed_as_limits(self):
        window = source("src/Host.Windows/MainWindow.xaml.cs")
        discovery = source("src/Host.Windows/WindowsHostHardwareDiscovery.cs")
        evaluator = source("src/Host.Application/HostReadinessEvaluator.cs")
        self.assertIn("WindowsHostHardwareDiscovery.Discover()", window)
        self.assertIn("HostReadinessEvaluator.Evaluate(", window)
        self.assertIn("AvailablePhysicalMemoryBytes", discovery)
        self.assertIn("SystemDriveAvailableBytes", discovery)
        self.assertIn("requiredModelMemoryGiB", evaluator)
        self.assertIn("requiredFreeDiskGiB", evaluator)
        self.assertIn("GPU/display adapters (display inventory only; compute support is not inferred)", window)

    def test_participation_and_single_inference_admission_remain_fail_closed(self):
        window = source("src/Host.Windows/MainWindow.xaml.cs")
        xaml = source("src/Host.Windows/MainWindow.xaml")
        policy = source("src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs")
        coordinator = source("src/Host.Windows/Requests/HostRequestCoordinator.cs")
        lifecycle = source("src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs")
        self.assertIn('x:Name="NetworkParticipationToggle"', xaml)
        self.assertNotRegex(xaml, r'<CheckBox x:Name="NetworkParticipationToggle"[^>]*IsChecked="True"')
        self.assertIn("TrustPeerAfterExplicitConfirmation", window)
        self.assertIn("MessageBoxResult.No", window)
        self.assertIn("HostRequestAdmissionFailure.InactiveParticipation", policy)
        self.assertIn("HostRequestAdmissionFailure.CapacityUnknown", policy)
        self.assertIn("new(1, 1)", coordinator)
        self.assertIn("WaitAsync(0, cancellationToken)", coordinator)
        self.assertIn("public async Task StopAsync()", lifecycle)
        self.assertIn("Volatile.Write(ref _active, 0)", lifecycle)

if __name__ == "__main__":
    unittest.main()
