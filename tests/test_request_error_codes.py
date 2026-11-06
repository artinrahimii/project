"""Regression checks for stable, sanitized request error categories."""

import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PROTO = (ROOT / "protocol/whitep/v1/messages.proto").read_text(encoding="utf-8")
HOST = (ROOT / "src/Host.Application/HostErrorPresentation.cs").read_text(encoding="utf-8")
ANDROID = (
    ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/errors/RequestErrorCode.kt"
).read_text(encoding="utf-8")
PARSER = (
    ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/RemoteRequestErrorParser.kt"
).read_text(encoding="utf-8")
TRANSPORT = (
    ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt"
).read_text(encoding="utf-8")
COLLECTOR = (
    ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceRoundResponseCollector.kt"
).read_text(encoding="utf-8")
VIEW_MODEL = (
    ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt"
).read_text(encoding="utf-8")
HISTORY_UI = (
    ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt"
).read_text(encoding="utf-8")


class RequestErrorCodeTests(unittest.TestCase):
    def test_existing_protocol_error_enum_remains_the_wire_contract(self):
        for name in (
            "PROTOCOL_ERROR_CODE_INVALID_MESSAGE",
            "PROTOCOL_ERROR_CODE_UNSUPPORTED_VERSION",
            "PROTOCOL_ERROR_CODE_UNSUPPORTED_MODEL",
            "PROTOCOL_ERROR_CODE_HOST_BUSY",
            "PROTOCOL_ERROR_CODE_REQUEST_EXPIRED",
            "PROTOCOL_ERROR_CODE_RESOURCE_LIMIT",
            "PROTOCOL_ERROR_CODE_INTERNAL",
        ):
            with self.subTest(name=name):
                self.assertIn(name, PROTO)
        self.assertIn("FromProtocolCode(ProtocolErrorCode code)", HOST)
        self.assertIn("fromProtocolCode(code: ProtocolErrorCode)", ANDROID)

    def test_windows_maps_local_exceptions_to_stable_codes_without_raw_details(self):
        for code in (
            "NETWORK_UNAVAILABLE",
            "MODEL_UNAVAILABLE",
            "RESOURCE_LIMIT",
            "VALIDATION_FAILED",
            "REQUEST_EXPIRED",
            "HOST_BUSY",
            "CANCELLED",
            "INTERNAL",
        ):
            with self.subTest(code=code):
                self.assertIn(code, HOST)
        self.assertIn("ToErrorCode(Exception exception)", HOST)
        self.assertIn("ToUserMessage(ToErrorCode(exception))", HOST)
        self.assertNotIn("exception.Message", HOST)
        self.assertNotIn("exception.ToString()", HOST)

    def test_android_parses_typed_peer_errors_and_drops_remote_message_text(self):
        self.assertIn("ProtocolError.parseFrom(payload)", PARSER)
        self.assertIn("HostBusy.parseFrom(payload)", PARSER)
        self.assertIn("UnsupportedModel.parseFrom(payload)", PARSER)
        self.assertNotIn("protocolError.message", PARSER)
        self.assertNotIn("busy.message", PARSER)
        self.assertNotIn("unsupported.message", PARSER)
        self.assertIn("RemoteRequestErrorParser.parse(payload)", TRANSPORT)
        self.assertIn("RequestExecutionException(remoteError.errorCode)", TRANSPORT)

    def test_each_failed_android_attempt_keeps_a_user_displayable_code(self):
        self.assertIn("val errorCode: RequestErrorCode?", VIEW_MODEL)
        self.assertIn("failure.errorCode.userMessage", VIEW_MODEL)
        self.assertIn("exception.toRequestErrorCode().userMessage", VIEW_MODEL)
        self.assertIn("RequestErrorCode.fromWireCode(storedCode)", HISTORY_UI)
        self.assertIn("exception.toRequestErrorCode()", COLLECTOR)


if __name__ == "__main__":
    unittest.main()
