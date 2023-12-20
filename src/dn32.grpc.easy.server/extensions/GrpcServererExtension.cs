using dn32.grpc.easy.server.authentication;
using dn32.grpc.easy.server.model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.Server;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace dn32.grpc.easy.server.extensions;

public static class GrpcServererExtension
{
    private static List<ValorInternoParaControladoresGrpc> Controllers { get; set; } = [];

    public static void AddGrpcController<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
    {
        Controllers.Add(new ValorInternoParaControladoresGrpc
        {
            InterfaceType = typeof(TService),
            ConcreteType = typeof(TImplementation)
        });
    }

    public static void AddGrpcServerDefaultInitialize(this IServiceCollection services, WebApplicationBuilder builder, int grpcPort, int? restPort = null)
    {
        RuntimeTypeModel.Default.IncludeDateTimeKind = true;

        services.AddGrpc();
        services.AddCodeFirstGrpc(config =>
        {
            config.EnableDetailedErrors = false;
            config.MaxReceiveMessageSize = 4 * 1024 * 1024; // 4MB
            config.MaxSendMessageSize = 4 * 1024 * 1024; // 4MB
            config.ResponseCompressionLevel = CompressionLevel.Optimal;
        });

        //services.AddCodeFirstGrpcReflection();
        services.AddAuthentication(GrpcAuthenticationHandler.SchemeName).AddScheme<GrpcAuthenticationSchemeOptions, GrpcAuthenticationHandler>(GrpcAuthenticationHandler.SchemeName, options => options.AlwaysAuthenticate = true);

        if (!Controllers.Any()) throw new Exception($"No gRPC controllers added. Use {nameof(GrpcServererExtension)}.{nameof(AddGrpcController)} to add gRPC controllers.");
        Controllers.ForEach(controladores => services.AddScoped(controladores.InterfaceType, controladores.ConcreteType));

        builder.WebHost.ConfigureKestrel((context, serverOptions) =>
        {
            serverOptions.ListenAnyIP(grpcPort, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);

            if (restPort != null)
            {
                serverOptions.ListenAnyIP(restPort.Value, listenOptions => listenOptions.Protocols = HttpProtocols.Http1);
            }
        });
    }

    public static void UseGrpcServerDefaultInitialize(this WebApplication app)
    {
        var tipo = typeof(GrpcEndpointRouteBuilderExtensions);
        var metodo = tipo.GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService));

        foreach (var item in Controllers)
        {
            var metodoComGenerico = metodo.MakeGenericMethod(item.ConcreteType);
            metodoComGenerico.Invoke(null, new[] { app });
        }

        //app.MapCodeFirstGrpcReflectionService();
    }
}