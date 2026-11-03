import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
PROTOCOL = ROOT / "src/Host.Windows/Networking/Libp2pPeerMessageProtocol.cs"


class Libp2pPeerMessageBufferLifetimeTests(unittest.TestCase):
    def test_successful_sends_retain_bounded_frame_buffers(self):
        source = PROTOCOL.read_text(encoding="utf-8")
        send = source.split("internal async Task SendMessageAsync(", 1)[1].split(
            "internal async Task<byte[]> ReceiveMessageAsync(", 1
        )[0]
        self.assertIn("RetainOutboundFrame(frame)", send)
        self.assertIn("ReleaseOutboundFrame(frame)", send)
        self.assertNotIn("finally\n            {\n                CryptographicOperations.ZeroMemory(frame);", send)
        self.assertIn("_maximumPendingOutboundFrames = inboundQueueCapacity", source)
        self.assertIn("_maximumPendingOutboundBytes", source)

    def test_response_and_stream_teardown_release_retained_frames(self):
        source = PROTOCOL.read_text(encoding="utf-8")
        receive = source.split("internal async Task<byte[]> ReceiveMessageAsync(", 1)[1].split(
            "internal async Task DisconnectCurrentSessionAsync(", 1
        )[0]
        connect = source.split("protected override async Task ConnectAsync(", 1)[1].split(
            "internal async Task SendMessageAsync(", 1
        )[0]
        dispose = source.split("public void Dispose()", 1)[1]
        self.assertIn("ClearPendingOutboundFrames()", receive)
        self.assertIn("ClearPendingOutboundFrames()", connect)
        self.assertIn("ClearPendingOutboundFrames()", dispose)
        self.assertIn("CryptographicOperations.ZeroMemory(frame)", source)


if __name__ == "__main__":
    unittest.main()
