using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using WhiteP.Shared.Contracts.V1;

namespace WhiteP.Host.Application;

/// <summary>Stable categories shared with the Android client and protocol error contract.</summary>
public enum HostErrorCode
{
    NETWORK_UNAVAILABLE,
    MODEL_UNAVAILABLE,
    RESOURCE_LIMIT,
    VALIDATION_FAILED,
    REQUEST_EXPIRED,
    HOST_BUSY,
    CANCELLED,
    INTERNAL
}

/// <summary>
/// Maps internal failures to stable codes and actionable messages without exposing exception text,
/// local paths, credentials, or server response details in the user interface.
/// </summary>
public static class HostErrorPresentation
{
    public static HostErrorCode ToErrorCode(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception switch
        {
            OperationCanceledException => HostErrorCode.CANCELLED,
            TimeoutException => HostErrorCode.REQUEST_EXPIRED,
            HttpRequestException or SocketException => HostErrorCode.NETWORK_UNAVAILABLE,
            FileNotFoundException or DirectoryNotFoundException => HostErrorCode.MODEL_UNAVAILABLE,
            OutOfMemoryException or UnauthorizedAccessException => HostErrorCode.RESOURCE_LIMIT,
            InvalidDataException or ArgumentException => HostErrorCode.VALIDATION_FAILED,
            IOException => HostErrorCode.RESOURCE_LIMIT,
            InvalidOperationException => HostErrorCode.HOST_BUSY,
            _ => HostErrorCode.INTERNAL
        };
    }

    public static HostErrorCode FromProtocolCode(ProtocolErrorCode code) =>
        code switch
        {
            ProtocolErrorCode.InvalidMessage or ProtocolErrorCode.UnsupportedVersion =>
                HostErrorCode.VALIDATION_FAILED,
            ProtocolErrorCode.UnsupportedModel => HostErrorCode.MODEL_UNAVAILABLE,
            ProtocolErrorCode.HostBusy => HostErrorCode.HOST_BUSY,
            ProtocolErrorCode.RequestExpired => HostErrorCode.REQUEST_EXPIRED,
            ProtocolErrorCode.ResourceLimit => HostErrorCode.RESOURCE_LIMIT,
            ProtocolErrorCode.Internal => HostErrorCode.INTERNAL,
            _ => HostErrorCode.INTERNAL
        };

    public static string ToUserMessage(Exception exception) => ToUserMessage(ToErrorCode(exception));

    public static string ToUserMessage(HostErrorCode code) =>
        code switch
        {
            HostErrorCode.NETWORK_UNAVAILABLE =>
                "Could not reach the local inference server. Check that it is running and the URL is correct.",
            HostErrorCode.MODEL_UNAVAILABLE =>
                "The selected model is unavailable. Check that it is installed and the model ID is correct.",
            HostErrorCode.RESOURCE_LIMIT =>
                "The app could not access a required local resource. Check file permissions and available memory or disk space.",
            HostErrorCode.VALIDATION_FAILED =>
                "Local settings or the server response were invalid. Check the configuration and local server.",
            HostErrorCode.REQUEST_EXPIRED =>
                "The request deadline expired. Prepare a new request.",
            HostErrorCode.HOST_BUSY =>
                "This operation is not available in the current state. Check the host status and try again.",
            HostErrorCode.CANCELLED =>
                "The operation was cancelled or timed out. Try again.",
            _ =>
                "An unexpected error occurred. Details are hidden to protect local paths and sensitive data."
        };
}
