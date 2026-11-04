namespace WhiteP.Shared.Security;

public static class PeerInferencePrivacyDisclosure
{
    public const string HostParticipationTitle = "Confirm peer inference privacy notice";

    public static string BuildHostParticipationMessage(string peerId)
    {
        if (string.IsNullOrWhiteSpace(peerId))
            throw new ArgumentException("A verified peer identity is required.", nameof(peerId));

        return
            $"This Windows host will accept authenticated inference requests from this exact peer:\n{peerId}\n\n" +
            "When a request arrives, this host process can read its prompt and context capsule after transport decryption. " +
            "The prompt is passed to the configured local inference backend.\n\n" +
            "Transport encryption protects data while it is in transit; it does not hide request content from this host, " +
            "the person operating this computer, or software with sufficient access on this device.\n\n" +
            "The application cannot control what the requesting peer or its operator does with the response. " +
            "Enable peer hosting only if you accept processing requests from this peer.";
    }
}
