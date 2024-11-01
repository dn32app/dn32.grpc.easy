using Microsoft.Extensions.DependencyInjection;

namespace dn32.grpc.easy.client.model;

public class GrpcControllerData
{
    public class RemoveControllerData
    {
        public required Type Type { get; set; }
        public required ServiceLifetime ServiceLifetime { get; set; }
        public required string ServerUrl { get; set; }
    }

    public required List<RemoveControllerData> Controllers { get; set; }
    public required List<Type> Interceptors { get; set; }
    public required GrpcRetryPolicy GrpcRetryPolicy { get; set; }
    public required GrpcSocketsHttpHandler GrpcSocketsHttpHandler { get; set; }
    public required IServiceCollection Services { get; set; }
}