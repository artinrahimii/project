"""Regression guards for transient peer-payload and frame-buffer cleanup."""

import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
HOST_PROTOCOL = ROOT / "src/Host.Windows/Networking/Libp2pPeerMessageProtocol.cs"
HOST_LIFECYCLE = ROOT / "src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs"
ANDROID_CODEC = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/PeerMessageFrameCodec.kt"
ANDROID_TRANSPORT = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt"


class SensitiveBufferCleanupTests(unittest.TestCase):
    def test_windows_transport_zeroes_outbound_frames_and_abandoned_queue_items(self):
        source = HOST_PROTOCOL.read_text(encoding="utf-8")
        self.assertIn("CryptographicOperations.ZeroMemory(frame)", source)
        self.assertIn("CryptographicOperations.ZeroMemory(message)", source)
        self.assertIn("_inbound.Writer.TryComplete()", source)
        self.assertIn("_inbound.Reader.TryRead(out byte[]? pendingMessage)", source)
        self.assertIn("pendingMessage is { Length: > 0 }", source)
        self.assertIn("CryptographicOperations.ZeroMemory(pendingMessage)", source)

    def test_windows_request_lifecycle_zeroes_serialized_and_handed_off_messages(self):
        source = HOST_LIFECYCLE.read_text(encoding="utf-8")
        self.assertIn("SendAndClearMessageAsync", source)
        self.assertIn("CryptographicOperations.ZeroMemory(wire)", source)
        self.assertIn("CryptographicOperations.ZeroMemory(requestWire)", source)
        self.assertIn("CryptographicOperations.ZeroMemory(responseWire)", source)

    def test_android_decoder_zeroes_replaced_temporary_and_partial_buffers(self):
        source = ANDROID_CODEC.read_text(encoding="utf-8")
        self.assertIn("Arrays.fill(previousPending, 0)", source)
        self.assertIn("Arrays.fill(input, 0)", source)
        self.assertIn("fun clear()", source)
        self.assertIn("Arrays.fill(previous, 0)", source)
        self.assertIn("val truncated = pending.isNotEmpty()", source)

    def test_android_controller_clears_all_frames_and_decoder_on_stream_close(self):
        source = ANDROID_TRANSPORT.read_text(encoding="utf-8")
        self.assertIn("frames.forEach { frame -> Arrays.fill(frame, 0) }", source)
        self.assertIn("decoder.clear()", source)
        self.assertIn("Arrays.fill(bytes, 0)", source)


if __name__ == "__main__":
    unittest.main()
