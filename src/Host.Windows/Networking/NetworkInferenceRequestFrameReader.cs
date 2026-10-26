using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WhiteP.Host.Infrastructure;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.V1;

namespace WhiteP.Host.Windows.Networking;

public static class NetworkInferenceRequestFrameReader
{
    public const int LengthPrefixBytes = sizeof(uint);
    public static async Task<NetworkInferenceRequestFrameResult> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var prefix = new byte[LengthPrefixBytes];
        var prefixRead = await ReadUntilFullAsync(stream, prefix, cancellationToken).ConfigureAwait(false);
        if (prefixRead == 0) return new(NetworkInferenceRequestFrameStatus.NoFrame, null, null);
        if (prefixRead != prefix.Length)
            return Invalid(NetworkInferenceRequestFrameStatus.TruncatedLengthPrefix, ProtocolMessageValidator.InvalidMessage("The TCP message length prefix is incomplete."));
        var payloadLength = BinaryPrimitives.ReadUInt32BigEndian(prefix);
        if (payloadLength == 0)
            return Invalid(NetworkInferenceRequestFrameStatus.InvalidLength, ProtocolMessageValidator.InvalidMessage("The TCP message payload length must be greater than zero."));
        if (payloadLength > ProtocolMessageLimits.MaximumRequestMessageBytes)
            return Invalid(NetworkInferenceRequestFrameStatus.FrameTooLarge, ProtocolMessageValidator.FromLimitResult(ProtocolMessageLimits.ValidateRequestMessageBytes(payloadLength)));
        var payload = new byte[(int)payloadLength];
        var payloadRead = await ReadUntilFullAsync(stream, payload, cancellationToken).ConfigureAwait(false);
        if (payloadRead != payload.Length)
            return Invalid(NetworkInferenceRequestFrameStatus.TruncatedPayload, ProtocolMessageValidator.InvalidMessage("The TCP message payload ended before its declared length."));
        var parsed = ProtocolMessageParser.ParseInferenceRequest(payload);
        if (!parsed.IsValid || parsed.Message is null)
            return Invalid(NetworkInferenceRequestFrameStatus.RejectedByProtocolParser, parsed.Validation);
        return new(NetworkInferenceRequestFrameStatus.Accepted, parsed.Message, parsed.Validation);
    }
    private static async Task<int> ReadUntilFullAsync(Stream stream, Memory<byte> destination, CancellationToken cancellationToken)
    {
        var totalRead = 0;
        while (totalRead < destination.Length)
        {
            var bytesRead = await stream.ReadAsync(destination[totalRead..], cancellationToken).ConfigureAwait(false);
            if (bytesRead == 0) break;
            totalRead += bytesRead;
        }
        return totalRead;
    }
    private static Task<int> ReadUntilFullAsync(Stream stream, byte[] destination, CancellationToken cancellationToken) =>
        ReadUntilFullAsync(stream, destination.AsMemory(), cancellationToken);
    private static NetworkInferenceRequestFrameResult Invalid(NetworkInferenceRequestFrameStatus status, ProtocolMessageValidationResult validation) =>
        new(status, null, validation);
}
public enum NetworkInferenceRequestFrameStatus { Accepted = 0, NoFrame = 1, TruncatedLengthPrefix = 2, InvalidLength = 3, FrameTooLarge = 4, TruncatedPayload = 5, RejectedByProtocolParser = 6 }
public readonly record struct NetworkInferenceRequestFrameResult(NetworkInferenceRequestFrameStatus Status, InferenceRequest? Request, ProtocolMessageValidationResult? Validation)
{
    public bool IsAccepted => Status == NetworkInferenceRequestFrameStatus.Accepted && Request is not null;
}
