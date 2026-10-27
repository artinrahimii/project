using System.Buffers;
using System.IO;
using System.Threading.Channels;
using Nethermind.Libp2p.Core;

namespace WhiteP.Host.Windows.Networking;

/// <summary>
/// Bounded, message-oriented application protocol carried over the libp2p negotiated
/// connection. Transport encryption and peer authentication are provided by the
/// libp2p TCP + Noise/TLS stack, not by this framing protocol.
/// </summary>
internal sealed class Libp2pPeerMessageProtocol : SymmetricSessionProtocol, ISessionProtocol
{
    private readonly int _maximumMessageBytes;
    private readonly Channel<byte[]> _inbound;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private IChannel? _channel;
    private Func<Task>? _disconnectCurrentSession;
    private Exception? _terminalError;
    private int _started;
    private int _connected;

    public Libp2pPeerMessageProtocol(int maximumMessageBytes, int inboundQueueCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumMessageBytes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(inboundQueueCapacity);
        _maximumMessageBytes = maximumMessageBytes;
        _inbound = System.Threading.Channels.Channel.CreateBounded<byte[]>(new BoundedChannelOptions(inboundQueueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = false
        });
    }

    public string Id => "/white-p/peer-message/1.0.0";

    internal Task Ready => _ready.Task;

    internal bool IsConnected => Volatile.Read(ref _connected) == 1;

    protected override async Task ConnectAsync(IChannel channel, ISessionContext context, bool isListener)
    {
        if (Interlocked.CompareExchange(ref _started, 1, 0) != 0)
        {
            await context.DisconnectAsync().ConfigureAwait(false);
            return;
        }

        _channel = channel;
        _disconnectCurrentSession = context.DisconnectAsync;
        Volatile.Write(ref _connected, 1);
        _ready.TrySetResult();
        // Acquire the channel token once. The interface's default implementation creates
        // a cancellation source when the property is read.
        CancellationToken channelToken = channel.CancellationToken;

        try
        {
            while (!channelToken.IsCancellationRequested)
            {
                int messageLength = await channel.ReadVarintAsync(channelToken).ConfigureAwait(false);
                if (messageLength < 0 || messageLength > _maximumMessageBytes)
                {
                    throw new InvalidDataException("The peer message length is outside the configured limit.");
                }

                byte[] message;
                if (messageLength == 0)
                {
                    message = Array.Empty<byte>();
                }
                else
                {
                    var read = await channel.ReadAsync(messageLength, token: channelToken).ConfigureAwait(false);
                    if (read.Result != IOResult.Ok)
                    {
                        throw new IOException("The peer message frame ended before the complete payload was received.");
                    }

                    message = read.Data.ToArray();
                    if (message.Length != messageLength)
                    {
                        throw new IOException("The peer message frame payload length did not match its prefix.");
                    }
                }

                await _inbound.Writer.WriteAsync(message, channelToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (channelToken.IsCancellationRequested)
        {
            // Normal shutdown/disconnect. The inbound reader will observe channel completion.
        }
        catch (Exception exception)
        {
            // Store only the exception object for local diagnostics; never log the message bytes
            // or application payload. A malformed/oversized frame is a fatal protocol error.
            _terminalError = exception;
            try
            {
                await context.DisconnectAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Preserve the original protocol failure; session teardown is best-effort here.
            }
        }
        finally
        {
            Volatile.Write(ref _connected, 0);
            _inbound.Writer.TryComplete(_terminalError);
        }
    }

    internal async Task SendMessageAsync(ReadOnlyMemory<byte> message, CancellationToken cancellationToken)
    {
        if (message.Length > _maximumMessageBytes)
        {
            throw new InvalidDataException("The peer message exceeds the configured size limit.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        await _sendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            IChannel channel = _channel ?? throw new IOException("The peer application channel is not connected.");
            if (!IsConnected)
            {
                throw new IOException("The peer application channel is closed.");
            }

            int prefixLength = VarInt.GetSizeInBytes(message.Length);
            byte[] frame = new byte[checked(prefixLength + message.Length)];
            int offset = 0;
            VarInt.Encode(message.Length, frame, ref offset);
            message.Span.CopyTo(frame.AsSpan(offset));

            IOResult result = await channel.WriteAsync(new ReadOnlySequence<byte>(frame), cancellationToken)
                .ConfigureAwait(false);
            if (result != IOResult.Ok)
            {
                throw new IOException("The peer transport did not accept the complete message frame.");
            }
        }
        finally
        {
            _sendLock.Release();
        }
    }

    internal async Task<byte[]> ReceiveMessageAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _inbound.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (ChannelClosedException exception)
        {
            Exception? terminalError = Volatile.Read(ref _terminalError);
            throw new IOException("The peer disconnected before another complete message was received.",
                terminalError ?? exception);
        }
    }

    internal async Task DisconnectCurrentSessionAsync()
    {
        Func<Task>? disconnect = Volatile.Read(ref _disconnectCurrentSession);
        if (disconnect is not null)
        {
            await disconnect().ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        _inbound.Writer.TryComplete();
        _sendLock.Dispose();
    }
}
