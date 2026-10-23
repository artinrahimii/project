using WhiteP.Host.Application.Inference;

internal static class InferenceEngineContractTests
{
    public static void Run()
    {
        var contract = typeof(IInferenceEngine);
        Require(typeof(IAsyncDisposable).IsAssignableFrom(contract),
            "IInferenceEngine must expose asynchronous resource disposal.");

        RequireMethod(nameof(IInferenceEngine.DiscoverAsync),
            typeof(Task<InferenceEngineDiscoveryResult>), typeof(CancellationToken));
        RequireMethod(nameof(IInferenceEngine.LoadModelAsync),
            typeof(Task), typeof(string), typeof(CancellationToken));
        RequireMethod(nameof(IInferenceEngine.GenerateAsync),
            typeof(Task<string>), typeof(string), typeof(uint), typeof(CancellationToken));
        RequireMethod(nameof(IInferenceEngine.CancelAsync), typeof(Task));
        RequireMethod(nameof(IInferenceEngine.UnloadModelAsync),
            typeof(Task), typeof(CancellationToken));

        foreach (var property in new[] { "BackendId", "DisplayName", "IsAvailable", "UnavailableReason" })
        {
            if (typeof(InferenceEngineDiscoveryResult).GetProperty(property) is null)
            {
                throw new Exception($"Discovery result contract is missing {property}.");
            }
        }

        Console.WriteLine("PASS: IInferenceEngine exposes discovery, model lifecycle, cancellable generation, explicit cancellation, and async disposal.");
    }

    private static void RequireMethod(string name, Type returnType, params Type[] parameterTypes)
    {
        var method = typeof(IInferenceEngine).GetMethod(name, parameterTypes);
        if (method is null || method.ReturnType != returnType)
        {
            throw new Exception($"IInferenceEngine.{name} does not match the required contract signature.");
        }
    }
}
