using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using WhiteP.Host.Application;
using WhiteP.Shared.Contracts.V1;

internal static class HostErrorPresentationAcceptanceTests
{
    public static void Run()
    {
        AssertMapping(new HttpRequestException("https://private.example/token"), HostErrorCode.NETWORK_UNAVAILABLE);
        AssertMapping(new SocketException(), HostErrorCode.NETWORK_UNAVAILABLE);
        AssertMapping(new FileNotFoundException("C:\\private\\model.gguf"), HostErrorCode.MODEL_UNAVAILABLE);
        AssertMapping(new DirectoryNotFoundException("C:\\private\\models"), HostErrorCode.MODEL_UNAVAILABLE);
        AssertMapping(new OutOfMemoryException("private allocation detail"), HostErrorCode.RESOURCE_LIMIT);
        AssertMapping(new UnauthorizedAccessException("C:\\private\\token.json"), HostErrorCode.RESOURCE_LIMIT);
        AssertMapping(new InvalidDataException("C:\\private\\settings.json"), HostErrorCode.VALIDATION_FAILED);
        AssertMapping(new ArgumentException("C:\\private\\settings.json"), HostErrorCode.VALIDATION_FAILED);
        AssertMapping(new TimeoutException("private deadline detail"), HostErrorCode.REQUEST_EXPIRED);
        AssertMapping(new InvalidOperationException("private host state"), HostErrorCode.HOST_BUSY);
        AssertMapping(new OperationCanceledException("private cancellation detail"), HostErrorCode.CANCELLED);
        AssertMapping(new Exception("C:\\private\\token.json"), HostErrorCode.INTERNAL);

        Assert(HostErrorPresentation.FromProtocolCode(ProtocolErrorCode.UnsupportedModel) == HostErrorCode.MODEL_UNAVAILABLE,
            "Unsupported-model protocol failures must use the stable model category.");
        Assert(HostErrorPresentation.FromProtocolCode(ProtocolErrorCode.ResourceLimit) == HostErrorCode.RESOURCE_LIMIT,
            "Protocol resource-limit failures must use the stable resource category.");
        Assert(HostErrorPresentation.FromProtocolCode(ProtocolErrorCode.InvalidMessage) == HostErrorCode.VALIDATION_FAILED,
            "Protocol validation failures must use the stable validation category.");
        Assert(HostErrorPresentation.FromProtocolCode(ProtocolErrorCode.HostBusy) == HostErrorCode.HOST_BUSY,
            "Host-busy protocol failures must use the stable busy category.");

        Console.WriteLine("PASS: Windows error categories and protocol-code mappings are stable and never expose exception details.");
    }

    private static void AssertMapping(Exception exception, HostErrorCode expected)
    {
        var actual = HostErrorPresentation.ToErrorCode(exception);
        Assert(actual == expected, $"Expected {expected}, received {actual}.");
        var message = HostErrorPresentation.ToUserMessage(exception);
        Assert(!message.Contains("private", StringComparison.OrdinalIgnoreCase) &&
               !message.Contains("token.json", StringComparison.OrdinalIgnoreCase) &&
               !message.Contains("model.gguf", StringComparison.OrdinalIgnoreCase),
            "A user-facing error message must not expose exception text or local paths.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
