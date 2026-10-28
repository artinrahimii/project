using System;
using System.IO;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.V1;

namespace WhiteP.Host.Windows.Networking;

/// <summary>
/// Checks a host-generated peer response against the request and local host identity before transmission.
/// </summary>
public static class PeerResponseCorrelationValidator
{
    public static void Validate(InferenceRequest request, InferenceResponse response, string expectedHostIdentity)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(response);

        if (string.IsNullOrWhiteSpace(expectedHostIdentity) ||
            !StringComparer.Ordinal.Equals(request.RequestId, response.RequestId) ||
            !StringComparer.Ordinal.Equals(request.AttemptId, response.AttemptId) ||
            !StringComparer.Ordinal.Equals(request.ModelId, response.ModelId) ||
            !StringComparer.Ordinal.Equals(expectedHostIdentity, response.EphemeralHostId))
        {
            throw new InvalidDataException("The peer response does not match the active request correlation.");
        }

        var validation = ProtocolMessageValidator.ValidateInferenceResponse(
            response.RequestId, response.AttemptId, response.EphemeralHostId, (int)response.Outcome,
            response.OutputText, response.CompletedAtUnixMs, response.ModelId, response.ErrorMessage,
            response.CalculateSize());
        if (!validation.IsValid)
            throw new InvalidDataException("The peer response does not satisfy the protocol message contract.");
    }
}
