using System.IO;
using System.Net.Http;

namespace WhiteP.Host.Application;

/// <summary>
/// Maps internal failures to actionable messages without exposing exception text,
/// local paths, credentials, or server response details in the user interface.
/// </summary>
public static class HostErrorPresentation
{
    public static string ToUserMessage(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception switch
        {
            OperationCanceledException => "The operation was cancelled or timed out. Try again.",
            HttpRequestException => "Could not reach the local inference server. Check that it is running and the URL is correct.",
            UnauthorizedAccessException => "The app could not access a required local resource. Check file permissions.",
            InvalidDataException => "Local settings or the server response were invalid. Check the configuration and local server.",
            ArgumentException => "Some settings are invalid. Check the server URL and model ID.",
            InvalidOperationException => "This operation is not available in the current state. Check the host status and try again.",
            _ => "An unexpected error occurred. Details are hidden to protect local paths and sensitive data."
        };
    }
}
