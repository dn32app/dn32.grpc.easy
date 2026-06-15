using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

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

    // GrpcChannel é caro e thread-safe: deve ser reutilizado, nunca recriado por requisição.
    // Cache por URL para evitar vazamento de conexões/SocketsHttpHandler.
    internal ConcurrentDictionary<string, GrpcChannel> ChannelCache { get; } = new();
}