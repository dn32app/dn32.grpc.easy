using dn32.grpc.easy.server.exceptions;
using dn32.grpc.easy.server.model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.Server;
using ProtoBuf.Meta;
using System.IO.Compression;

namespace dn32.grpc.easy.server.extensions;

public static class GrpcServererExtension
{
    public static List<InternalValuesForGrpcControllers> InitGrptServer(this IServiceCollection services)
    {
        return [];
    }

    public static IServiceCollection AddGrpcController<TService, TImplementation>(this IServiceCollection services, List<InternalValuesForGrpcControllers> controllers) where TService : class where TImplementation : class, TService
    {
        controllers.Add(new InternalValuesForGrpcControllers
        {
            InterfaceType = typeof(TService),
            ConcreteType = typeof(TImplementation)
        });

        return services;
    }

    public static IServiceCollection AddGrpcServerDefaultInitialize(this IServiceCollection services, List<InternalValuesForGrpcControllers> controllers, WebApplicationBuilder builder, int grpcPort, int? restPort = null, List<Type>? interceptors = null, GrpcServerConfig? configValue = null)
    {
        RuntimeTypeModel.Default.IncludeDateTimeKind = true;

        services.AddGrpc();
        services.AddCodeFirstGrpc(config =>
        {
            config.EnableDetailedErrors = configValue?.EnableDetailedErrors ?? true;
            config.MaxReceiveMessageSize = configValue?.MaxReceiveMessageSize ?? 20 * 1024 * 1024; // 20MB
            config.MaxSendMessageSize = configValue?.MaxSendMessageSize ?? 20 * 1024 * 1024; // 20MB
            config.ResponseCompressionLevel = configValue?.ResponseCompressionLevel ?? CompressionLevel.Optimal;
            interceptors ??= [];
            interceptors.Add(typeof(ExceptionInterceptor));

            foreach (var interceptor in interceptors)
            {
                config.Interceptors.Add(interceptor);
            }
        });

        if (!controllers.Any()) throw new Exception($"No gRPC controllers added. Use {nameof(GrpcServererExtension)}.{nameof(AddGrpcController)} to add gRPC controllers.");
        controllers.ForEach(controladores => services.AddScoped(controladores.InterfaceType, controladores.ConcreteType));

        builder.WebHost.ConfigureKestrel((context, options) => { });

        builder.WebHost.UseKestrel().ConfigureKestrel((context, serverOptions) =>
        {
            serverOptions.ListenAnyIP(grpcPort, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);

            if (restPort != null)
            {
                serverOptions.ListenAnyIP(restPort.Value, listenOptions => listenOptions.Protocols = HttpProtocols.Http1);
            }
        });

        return services;
    }

    public static void UseGrpcServerDefaultInitialize(this WebApplication app, List<InternalValuesForGrpcControllers> controllers)
    {
        var tipo = typeof(GrpcEndpointRouteBuilderExtensions);
        var metodo = tipo.GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService));

        foreach (var item in controllers)
        {
            var metodoComGenerico = metodo?.MakeGenericMethod(item.ConcreteType);
            metodoComGenerico?.Invoke(null, new[] { app });
        }
    }
}