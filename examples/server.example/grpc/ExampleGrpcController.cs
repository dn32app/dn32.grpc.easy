using shared.example.contract;

namespace server.example.grpc;

public class ExampleGrpcController(IHttpContextAccessor httpContextAccessor) : IExampleGrpcController
{
    public async Task<GrpcResultContractExampleV1> SimpleExampleAsync(GrpcContractExampleV1 exampleGrpcContract)
    {
        var user = httpContextAccessor.HttpContext.User;
        await Task.CompletedTask;
        return new GrpcResultContractExampleV1 { Result = $"{exampleGrpcContract.Name}, {string.Join(",", exampleGrpcContract.Metadata?.Select(m => $"{m.Key}: {m.Value}") ?? [])}" };
    }
}