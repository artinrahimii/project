using System;
using System.Threading;
using System.Threading.Tasks;
using WhiteP.Host.Windows.Networking;

internal static class PeerTransportContractAcceptanceTests
{
    public static Task RunAsync()
    {
        VerifyInterfaceExposesTheRequiredSecureLifecycle();
        VerifyEndpointRequiresAddressAndExpectedIdentity();
        Console.WriteLine("PASS: IPeerTransport exposes a secure-connect/message-send/message-receive/disconnect contract and validates its expected peer endpoint.");
        return Task.CompletedTask;
    }

    private static void VerifyInterfaceExposesTheRequiredSecureLifecycle()
    {
        Assert(typeof(IPeerTransport).IsInterface, "IPeerTransport must be an abstraction, not a concrete transport implementation.");
        Assert(typeof(IAsyncDisposable).IsAssignableFrom(typeof(IPeerTransport)), "IPeerTransport must support asynchronous resource disposal.");
        Assert(typeof(IPeerTransport).GetProperty(nameof(IPeerTransport.IsConnected))?.PropertyType == typeof(bool),
            "IsConnected must expose the authenticated connection state.");
        Assert(typeof(IPeerTransport).GetProperty(nameof(IPeerTransport.RemotePeerIdentity))?.PropertyType == typeof(string),
            "RemotePeerIdentity must expose the authenticated remote identity.");

        AssertMethod(nameof(IPeerTransport.ConnectSecureAsync), typeof(Task), typeof(PeerTransportEndpoint), typeof(CancellationToken));
        AssertMethod(nameof(IPeerTransport.SendMessageAsync), typeof(Task), typeof(ReadOnlyMemory<byte>), typeof(CancellationToken));
        AssertMethod(nameof(IPeerTransport.ReceiveMessageAsync), typeof(Task<byte[]>), typeof(CancellationToken));
        AssertMethod(nameof(IPeerTransport.DisconnectAsync), typeof(Task), typeof(CancellationToken));
    }

    private static void AssertMethod(string name, Type returnType, params Type[] parameterTypes)
    {
        var method = typeof(IPeerTransport).GetMethod(name, parameterTypes);
        Assert(method is not null, $"IPeerTransport is missing required method {name}.");
        Assert(method!.ReturnType == returnType, $"{name} has an unexpected return type.");
    }

    private static void VerifyEndpointRequiresAddressAndExpectedIdentity()
    {
        var endpoint = new PeerTransportEndpoint("/ip4/192.0.2.15/tcp/43001", "peer-ephemeral-01");
        Assert(endpoint.Address == "/ip4/192.0.2.15/tcp/43001", "Transport address must be preserved.");
        Assert(endpoint.ExpectedPeerIdentity == "peer-ephemeral-01", "Expected peer identity must be preserved.");

        ExpectArgumentException(() => new PeerTransportEndpoint("", "peer-ephemeral-01"),
            "A blank transport address must be rejected.");
        ExpectArgumentException(() => new PeerTransportEndpoint(" /ip4/192.0.2.15/tcp/43001", "peer-ephemeral-01"),
            "An address with surrounding whitespace must be rejected.");
        ExpectArgumentException(() => new PeerTransportEndpoint("/ip4/192.0.2.15/tcp/43001", " "),
            "A missing expected peer identity must be rejected.");
        ExpectArgumentException(() => new PeerTransportEndpoint("/ip4/192.0.2.15/tcp/43001", "peer\nmalicious"),
            "An expected peer identity containing control characters must be rejected.");
    }

    private static void ExpectArgumentException(Action action, string message)
    {
        try
        {
            action();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
