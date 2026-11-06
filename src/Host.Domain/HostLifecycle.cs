namespace WhiteP.Host.Domain;

public enum HostState
{
    Stopped,
    Starting,
    Ready,
    Busy,
    Error
}

public sealed record HostStatus(HostState State, string Message);

public sealed class HostLifecycle
{
    private static readonly IReadOnlyDictionary<HostState, HostState[]> Allowed = new Dictionary<HostState, HostState[]>
    {
        [HostState.Stopped] = [HostState.Starting],
        [HostState.Starting] = [HostState.Ready, HostState.Error, HostState.Stopped],
        [HostState.Ready] = [HostState.Busy, HostState.Error, HostState.Stopped],
        [HostState.Busy] = [HostState.Ready, HostState.Error],
        [HostState.Error] = [HostState.Starting, HostState.Stopped]
    };

    public HostStatus Status { get; private set; } = new(HostState.Stopped, "Host stopped.");

    public HostStatus Transition(HostState next, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        if (!Allowed[Status.State].Contains(next))
        {
            throw new InvalidOperationException($"Invalid host lifecycle transition: {Status.State} -> {next}.");
        }

        return Status = new HostStatus(next, message.Trim());
    }
}
